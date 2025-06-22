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

namespace NET_API.Services
{
  public class PdfExportService
  {
    private readonly ChartService _chartService;

    public PdfExportService(ChartService chartService)
    {
      _chartService = chartService;
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

        // 使用標準字體，避免中文字體問題
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // 添加標題 - 使用英文避免字體問題
        AddTitle(document, "TaiPower Power Consumption Report", font);

        // 添加折線圖
        AddChartImage(document, data);

        // 添加資料表格 - 使用英文標題
        AddDataTable(document, data, font);

        // 添加統計資訊 - 使用英文
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
    /// 添加圖表圖片
    /// </summary>
    private void AddChartImage(Document document, PowerDataResponse data)
    {
      var chartImageBytes = _chartService.GenerateTaiPowerLineChartAsPng(data);
      if (chartImageBytes == null) return;

      using (var imageStream = new MemoryStream(chartImageBytes))
      {
        var imageData = iText.IO.Image.ImageDataFactory.Create(imageStream.ToArray());
        var image = new Image(imageData);
        image.SetWidth(500);
        image.SetHeight(300);
        image.SetHorizontalAlignment(HorizontalAlignment.CENTER);
        image.SetMarginBottom(20);
        document.Add(image);
      }
    }

    /// <summary>
    /// 添加資料表格
    /// </summary>
    private void AddDataTable(Document document, PowerDataResponse data, PdfFont font)
    {
      var table = new Table(5);
      table.SetWidth(UnitValue.CreatePercentValue(100));

      // 表格標題 - 使用英文
      var headers = new[] { "Time", "East (MW)", "Central (MW)", "North (MW)", "South (MW)" };

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
      var stats = new Paragraph($"Total Records: {data.Count}, Export Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        .SetFont(font)
        .SetFontSize(10)
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetMarginTop(20);
      document.Add(stats);
    }
  }
}