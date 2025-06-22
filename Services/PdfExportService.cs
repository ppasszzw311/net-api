using NET_API.Dtos.TaiPower;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using Microsoft.Extensions.Hosting;
using iText.IO.Image;
using System.Threading.Tasks;

namespace NET_API.Services
{
  public class PdfExportService
  {
    private readonly HtmlChartService _htmlChartService;
    private readonly TaiPowerChartService _taipowerChartService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<PdfExportService> _logger;
    private PdfFont? _chineseFont;

    public PdfExportService(
        HtmlChartService htmlChartService, 
        TaiPowerChartService taipowerChartService,
        IHostEnvironment environment,
        ILogger<PdfExportService> logger)
    {
      _htmlChartService = htmlChartService;
      _taipowerChartService = taipowerChartService;
      _environment = environment;
      _logger = logger;
    }

    /// <summary>
    /// 取得中文字體
    /// </summary>
    /// <returns>PDF 字體物件</returns>
    private PdfFont GetChineseFont()
    {
      if (_chineseFont != null)
        return _chineseFont;

      var fontPaths = new[]
      {
        Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "SourceHanSans-Regular.ttf"),
        Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "NotoSansCJK-Regular.ttf")
      };

      foreach (var fontPath in fontPaths)
      {
        try
        {
          if (File.Exists(fontPath))
          {
            _chineseFont = PdfFontFactory.CreateFont(fontPath, "Identity-H");
            Console.WriteLine($"成功載入 PDF 中文字體: {fontPath}");
            return _chineseFont;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"載入 PDF 中文字體失敗: {ex.Message}");
        }
      }

      // 如果無法載入中文字體，使用預設字體
      _chineseFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      Console.WriteLine("使用預設字體替代中文字體");
      return _chineseFont;
    }

    /// <summary>
    /// 匯出台電資料為 PDF
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>PDF 檔案的 byte 陣列</returns>
    public async Task<byte[]> ExportTaiPowerDataToPdfAsync(PowerDataResponse data, string fileName = "TaiPowerData")
    {
      using (var stream = new MemoryStream())
      {
        using (var writer = new PdfWriter(stream))
        using (var pdf = new PdfDocument(writer))
        using (var document = new Document(pdf))
        {
          // 設定文件標題
          document.Add(new Paragraph("台電用電資料報告")
            .SetFont(GetChineseFont())
            .SetFontSize(18)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20));

          // 添加生成時間
          document.Add(new Paragraph($"報告生成時間: {DateTime.Now:yyyy/MM/dd HH:mm:ss}")
            .SetFont(GetChineseFont())
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetMarginBottom(20));

          // 添加圖表
          await AddChartImageAsync(document, data);

          // 添加資料表格
          AddDataTable(document, data, GetChineseFont());
        }

        return stream.ToArray();
      }
    }

    /// <summary>
    /// 添加標題
    /// </summary>
    private void AddTitle(Document document, string titleText, PdfFont font)
    {
      var title = new Paragraph(titleText)
        .SetFont(font)
        .SetFontSize(18)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginBottom(20);
      document.Add(title);
    }

    /// <summary>
    /// 添加圖表圖片到 PDF
    /// </summary>
    /// <param name="document">PDF 文件</param>
    /// <param name="data">台電資料</param>
    private async Task AddChartImageAsync(Document document, PowerDataResponse data)
    {
      try
      {
        // 添加圖表標題
        document.Add(new Paragraph("用電量趨勢圖表")
          .SetFont(GetChineseFont())
          .SetFontSize(14)
          .SetTextAlignment(TextAlignment.CENTER)
          .SetMarginBottom(10));

        byte[]? chartBytes = null;

        // 優先使用 Draw API 服務生成圖表
        try
        {
          _logger.LogInformation("嘗試使用 Draw API 生成PDF圖表");
          chartBytes = await _taipowerChartService.GenerateChartPngAsync(data.Data);
          _logger.LogInformation("Draw API 圖表生成成功，大小: {Size} bytes", chartBytes?.Length ?? 0);
        }
        catch (Exception drawApiEx)
        {
          _logger.LogWarning(drawApiEx, "Draw API 生成圖表失敗，嘗試使用備用方案");
          
          // Draw API 失敗時，使用原有的 HTML Chart Service 作為備用方案
          try
          {
            _logger.LogInformation("使用 HTML Chart Service 作為備用方案");
            chartBytes = await _htmlChartService.GenerateTaiPowerLineChartAsPngAsync(data);
            _logger.LogInformation("備用圖表生成成功，大小: {Size} bytes", chartBytes?.Length ?? 0);
          }
          catch (Exception htmlEx)
          {
            _logger.LogError(htmlEx, "備用圖表生成也失敗");
          }
        }

        // 如果成功生成圖表，添加到PDF
        if (chartBytes != null && chartBytes.Length > 0)
        {
          var imageData = ImageDataFactory.Create(chartBytes);
          var image = new Image(imageData);
          
          // 設定圖片尺寸和位置
          image.SetWidth(520); // 稍微增加寬度以提供更好的可讀性
          image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
          image.SetMarginBottom(20);
          
          document.Add(image);
          
          // 添加圖表說明
          document.Add(new Paragraph("圖表顯示四個地區（東部、中部、北部、南部）的用電量趨勢變化")
            .SetFont(GetChineseFont())
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20)
            .SetFontColor(ColorConstants.GRAY));
        }
        else
        {
          // 如果兩種方法都無法生成圖表，顯示詳細的錯誤信息
          _logger.LogError("所有圖表生成方法都失敗");
          
          var errorParagraph = new Paragraph("圖表生成暫時不可用")
            .SetFont(GetChineseFont())
            .SetFontSize(12)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetBorder(new SolidBorder(ColorConstants.RED, 1))
            .SetPadding(10)
            .SetMarginBottom(20);
          
          document.Add(errorParagraph);
          
          // 添加替代的簡單統計表格
          AddSimpleStatistics(document, data);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "添加圖表到 PDF 時發生未預期的錯誤");
        
        // 添加錯誤說明
        var errorParagraph = new Paragraph($"圖表載入失敗: {ex.Message}")
          .SetFont(GetChineseFont())
          .SetFontSize(10)
          .SetTextAlignment(TextAlignment.CENTER)
          .SetBorder(new SolidBorder(ColorConstants.RED, 1))
          .SetPadding(10)
          .SetMarginBottom(20);
        
        document.Add(errorParagraph);
      }
    }

    /// <summary>
    /// 添加資料表格
    /// </summary>
    private void AddDataTable(Document document, PowerDataResponse data, PdfFont font)
    {
      var table = new Table(5);
      table.SetWidth(UnitValue.CreatePercentValue(100));

      // 表格標題 - 使用繁體中文
      var headers = new[] { "時間", "東部 (MW)", "中部 (MW)", "北部 (MW)", "南部 (MW)" };

      foreach (var header in headers)
      {
        var cell = new Cell()
          .Add(new Paragraph(header).SetFont(font).SetFontSize(10))
          .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
          .SetTextAlignment(TextAlignment.CENTER)
          .SetPadding(5);
        table.AddHeaderCell(cell);
      }

      // 資料行
      foreach (var powerData in data.Data.Take(30)) // 限制前30筆資料避免PDF過大
      {
        table.AddCell(new Cell()
          .Add(new Paragraph(powerData.Time.ToString("yyyy-MM-dd HH:mm:ss")).SetFont(font).SetFontSize(8))
          .SetTextAlignment(TextAlignment.CENTER)
          .SetPadding(3));

        table.AddCell(new Cell()
          .Add(new Paragraph((powerData.EastConsumption?.ToString("F2") ?? "0.00")).SetFont(font).SetFontSize(8))
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetPadding(3));

        table.AddCell(new Cell()
          .Add(new Paragraph((powerData.CentralConsumption?.ToString("F2") ?? "0.00")).SetFont(font).SetFontSize(8))
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetPadding(3));

        table.AddCell(new Cell()
          .Add(new Paragraph((powerData.NorthConsumption?.ToString("F2") ?? "0.00")).SetFont(font).SetFontSize(8))
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetPadding(3));

        table.AddCell(new Cell()
          .Add(new Paragraph((powerData.SouthConsumption?.ToString("F2") ?? "0.00")).SetFont(font).SetFontSize(8))
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetPadding(3));
      }

      document.Add(table);
    }

    /// <summary>
    /// 添加統計資訊
    /// </summary>
    private void AddStatistics(Document document, PowerDataResponse data, PdfFont font)
    {
      var stats = new Paragraph($"總計 {data.Count} 筆資料，匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        .SetFont(font)
        .SetFontSize(10)
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetMarginTop(20);
      document.Add(stats);
    }

    /// <summary>
    /// 添加簡單統計表格（當圖表無法生成時的替代方案）
    /// </summary>
    private void AddSimpleStatistics(Document document, PowerDataResponse data)
    {
      if (data.Data.Count == 0) return;

      // 計算各地區的統計數據
      var eastAvg = data.Data.Where(d => d.EastConsumption.HasValue).Average(d => d.EastConsumption.Value);
      var centralAvg = data.Data.Where(d => d.CentralConsumption.HasValue).Average(d => d.CentralConsumption.Value);
      var northAvg = data.Data.Where(d => d.NorthConsumption.HasValue).Average(d => d.NorthConsumption.Value);
      var southAvg = data.Data.Where(d => d.SouthConsumption.HasValue).Average(d => d.SouthConsumption.Value);

      var eastMax = data.Data.Where(d => d.EastConsumption.HasValue).Max(d => d.EastConsumption.Value);
      var centralMax = data.Data.Where(d => d.CentralConsumption.HasValue).Max(d => d.CentralConsumption.Value);
      var northMax = data.Data.Where(d => d.NorthConsumption.HasValue).Max(d => d.NorthConsumption.Value);
      var southMax = data.Data.Where(d => d.SouthConsumption.HasValue).Max(d => d.SouthConsumption.Value);

      // 創建統計表格
      var statsTable = new Table(3);
      statsTable.SetWidth(UnitValue.CreatePercentValue(80));
      statsTable.SetHorizontalAlignment(HorizontalAlignment.CENTER);
      statsTable.SetMarginBottom(20);

      // 表頭
      var headers = new[] { "地區", "平均用電量 (MW)", "最大用電量 (MW)" };
      foreach (var header in headers)
      {
        var cell = new Cell()
          .Add(new Paragraph(header).SetFont(GetChineseFont()).SetFontSize(10))
          .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
          .SetTextAlignment(TextAlignment.CENTER)
          .SetPadding(5);
        statsTable.AddHeaderCell(cell);
      }

      // 統計數據行
      var regions = new[]
      {
        ("東部", eastAvg, eastMax),
        ("中部", centralAvg, centralMax),
        ("北部", northAvg, northMax),
        ("南部", southAvg, southMax)
      };

      foreach (var (region, avg, max) in regions)
      {
        statsTable.AddCell(new Cell()
          .Add(new Paragraph(region).SetFont(GetChineseFont()).SetFontSize(9))
          .SetTextAlignment(TextAlignment.CENTER)
          .SetPadding(3));

        statsTable.AddCell(new Cell()
          .Add(new Paragraph(avg.ToString("F2")).SetFont(GetChineseFont()).SetFontSize(9))
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetPadding(3));

        statsTable.AddCell(new Cell()
          .Add(new Paragraph(max.ToString("F2")).SetFont(GetChineseFont()).SetFontSize(9))
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetPadding(3));
      }

      // 添加說明和表格
      document.Add(new Paragraph("統計數據總覽")
        .SetFont(GetChineseFont())
        .SetFontSize(12)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetMarginBottom(10));

      document.Add(statsTable);
    }
  }
}