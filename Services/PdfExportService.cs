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

namespace NET_API.Services
{
  public class PdfExportService
  {
    private readonly ChartService _chartService;
    private readonly IHostEnvironment _environment;
    private PdfFont? _chineseFont;

    public PdfExportService(ChartService chartService, IHostEnvironment environment)
    {
      _chartService = chartService;
      _environment = environment;
    }

    /// <summary>
    /// 取得中文字體
    /// </summary>
    /// <returns>PDF 字體物件</returns>
    private PdfFont GetChineseFont()
    {
      if (_chineseFont != null)
        return _chineseFont;

      try
      {
        var fontPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "NotoSansCJK-Regular.ttf");
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

      // 如果無法載入中文字體，使用預設字體
      _chineseFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      return _chineseFont;
    }

    /// <summary>
    /// 匯出台電資料為 PDF 文件
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>PDF 文件的 byte 陣列</returns>
    public byte[] ExportTaiPowerDataToPdf(PowerDataResponse data, string fileName = "TaiPowerData")
    {
      using (var stream = new MemoryStream())
      {
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        // 使用中文字體
        PdfFont font = GetChineseFont();

        // 添加標題 - 使用繁體中文
        AddTitle(document, "台電用電量資料報告", font);

        // 添加折線圖
        AddChartImage(document, data);

        // 添加資料表格 - 使用繁體中文標題
        AddDataTable(document, data, font);

        // 添加統計資訊 - 使用繁體中文
        AddStatistics(document, data, font);

        document.Close();
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
    private void AddChartImage(Document document, PowerDataResponse data)
    {
      try
      {
        var chartBytes = _chartService.GenerateTaiPowerLineChartAsPng(data);
        if (chartBytes != null && chartBytes.Length > 0)
        {
          var imageData = ImageDataFactory.Create(chartBytes);
          var image = new Image(imageData);
          image.SetWidth(500);
          image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
          
          document.Add(new Paragraph("用電量趨勢圖表").SetFont(GetChineseFont()).SetFontSize(14));
          document.Add(image);
        }
        else
        {
          // 如果無法生成圖表，添加說明文字
          document.Add(new Paragraph("圖表生成功能暫時不可用").SetFont(GetChineseFont()).SetFontSize(12));
          Console.WriteLine("圖表生成失敗，在 PDF 中添加說明文字");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"添加圖表到 PDF 時發生錯誤: {ex.Message}");
        // 添加錯誤說明
        document.Add(new Paragraph("圖表載入失敗").SetFont(GetChineseFont()).SetFontSize(12));
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
  }
}