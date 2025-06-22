using NET_API.Dtos.TaiPower;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;

namespace NET_API.Services
{
  public class WordExportService
  {
    private readonly ChartService _chartService;

    public WordExportService(ChartService chartService)
    {
      _chartService = chartService;
    }

    /// <summary>
    /// 匯出台電資料為 Word 文件
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>Word 文件的 byte 陣列</returns>
    public byte[] ExportTaiPowerDataToWord(PowerDataResponse data, string fileName = "TaiPowerData")
    {
      using (var stream = new MemoryStream())
      {
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
          var mainPart = document.AddMainDocumentPart();
          mainPart.Document = new Document();
          var body = mainPart.Document.AppendChild(new Body());

          // 添加標題
          AddTitle(body, "台電用電量資料報告");

          // 添加說明文字
          AddDescription(body);

          // 添加資料表格
          AddDataTable(body, data);

          // 添加統計資訊
          AddStatistics(body, data);
        }

        return stream.ToArray();
      }
    }

    /// <summary>
    /// 添加標題
    /// </summary>
    private void AddTitle(Body body, string titleText)
    {
      var title = new Paragraph(
        new Run(
          new Text(titleText)
        )
      );
      title.ParagraphProperties = new ParagraphProperties(
        new Justification() { Val = JustificationValues.Center }
      );
      title.GetFirstChild<Run>().RunProperties = new RunProperties(
        new FontSize() { Val = "36" },
        new Bold()
      );
      body.AppendChild(title);
      body.AppendChild(new Paragraph()); // 空行
    }

    /// <summary>
    /// 添加說明文字
    /// </summary>
    private void AddDescription(Body body)
    {
      var description = new Paragraph(
        new Run(
          new Text("本報告包含台灣電力公司（台電）各地區的用電量資料。")
        )
      );
      description.ParagraphProperties = new ParagraphProperties(
        new Justification() { Val = JustificationValues.Left }
      );
      description.GetFirstChild<Run>().RunProperties = new RunProperties(
        new FontSize() { Val = "12" }
      );
      body.AppendChild(description);
      body.AppendChild(new Paragraph()); // 空行
    }

    /// <summary>
    /// 添加資料表格
    /// </summary>
    private void AddDataTable(Body body, PowerDataResponse data)
    {
      var table = new Table();

      // 表格屬性
      var tableProperties = new TableProperties(
        new TableBorders(
          new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
          new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
          new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
          new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
          new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
          new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 }
        )
      );
      table.AppendChild(tableProperties);

      // 表格標題行
      var headerRow = new TableRow();
      var headers = new[] { "時間", "東部用電量 (MW)", "中部用電量 (MW)", "北部用電量 (MW)", "南部用電量 (MW)" };

      foreach (var header in headers)
      {
        var cell = new TableCell(new Paragraph(new Run(new Text(header))));
        cell.TableCellProperties = new TableCellProperties(
          new TableCellWidth() { Width = "2000", Type = TableWidthUnitValues.Pct }
        );
        headerRow.AppendChild(cell);
      }
      table.AppendChild(headerRow);

      // 資料行
      foreach (var powerData in data.Data.Take(30)) // 限制前30筆資料
      {
        var row = new TableRow();

        row.AppendChild(new TableCell(new Paragraph(new Run(new Text(powerData.Time.ToString("yyyy-MM-dd HH:mm:ss"))))));
        row.AppendChild(new TableCell(new Paragraph(new Run(new Text((powerData.EastConsumption?.ToString("F2") ?? "0.00"))))));
        row.AppendChild(new TableCell(new Paragraph(new Run(new Text((powerData.CentralConsumption?.ToString("F2") ?? "0.00"))))));
        row.AppendChild(new TableCell(new Paragraph(new Run(new Text((powerData.NorthConsumption?.ToString("F2") ?? "0.00"))))));
        row.AppendChild(new TableCell(new Paragraph(new Run(new Text((powerData.SouthConsumption?.ToString("F2") ?? "0.00"))))));

        table.AppendChild(row);
      }

      body.AppendChild(table);
      body.AppendChild(new Paragraph()); // 空行
    }

    /// <summary>
    /// 添加統計資訊
    /// </summary>
    private void AddStatistics(Body body, PowerDataResponse data)
    {
      var stats = new Paragraph(
        new Run(
          new Text($"總計 {data.Count} 筆資料，匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        )
      );
      stats.ParagraphProperties = new ParagraphProperties(
        new Justification() { Val = JustificationValues.Right }
      );
      body.AppendChild(stats);
    }
  }
}