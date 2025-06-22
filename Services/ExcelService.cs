using OfficeOpenXml;
using NET_API.Dtos.TaiPower;
using System.IO;

namespace NET_API.Services
{
  public class ExcelService
  {
    public ExcelService()
    {
      // 設置 EPPlus 許可證（EPPlus 7.x 版本）
      ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// 匯出台電資料為 Excel 文件
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>Excel 文件的 byte 陣列</returns>
    public byte[] ExportTaiPowerDataToExcel(PowerDataResponse data, string fileName = "TaiPowerData")
    {
      using (var package = new ExcelPackage())
      {
        var worksheet = package.Workbook.Worksheets.Add("台電資料");

        // 設置標題行
        worksheet.Cells[1, 1].Value = "時間";
        worksheet.Cells[1, 2].Value = "東部用電量 (MW)";
        worksheet.Cells[1, 3].Value = "中部用電量 (MW)";
        worksheet.Cells[1, 4].Value = "北部用電量 (MW)";
        worksheet.Cells[1, 5].Value = "南部用電量 (MW)";

        // 設置標題行樣式
        using (var range = worksheet.Cells[1, 1, 1, 5])
        {
          range.Style.Font.Bold = true;
          range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
          range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
          range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        }

        // 填充資料
        for (int i = 0; i < data.Data.Count; i++)
        {
          var row = i + 2; // 從第2行開始（第1行是標題）
          var powerData = data.Data[i];

          worksheet.Cells[row, 1].Value = powerData.Time.ToString("yyyy-MM-dd HH:mm:ss");
          worksheet.Cells[row, 2].Value = powerData.EastConsumption;
          worksheet.Cells[row, 3].Value = powerData.CentralConsumption;
          worksheet.Cells[row, 4].Value = powerData.NorthConsumption;
          worksheet.Cells[row, 5].Value = powerData.SouthConsumption;

          // 設置數字格式
          worksheet.Cells[row, 2, row, 5].Style.Numberformat.Format = "#,##0.00";
        }

        // 自動調整欄寬
        worksheet.Cells.AutoFitColumns();

        // 設置時間欄位的格式
        worksheet.Column(1).Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

        // 添加邊框
        using (var range = worksheet.Cells[1, 1, data.Data.Count + 1, 5])
        {
          range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
          range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
          range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
          range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

        // 凍結標題行
        worksheet.View.FreezePanes(2, 1);

        return package.GetAsByteArray();
      }
    }

    /// <summary>
    /// 匯出特定地區的台電資料為 Excel 文件
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <param name="region">地區</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>Excel 文件的 byte 陣列</returns>
    public byte[] ExportTaiPowerDataByRegionToExcel(PowerDataResponse data, string region, string fileName = "TaiPowerData")
    {
      using (var package = new ExcelPackage())
      {
        var worksheet = package.Workbook.Worksheets.Add($"{GetRegionName(region)}地區資料");

        // 設置標題行
        worksheet.Cells[1, 1].Value = "時間";
        worksheet.Cells[1, 2].Value = $"{GetRegionName(region)}用電量 (MW)";

        // 設置標題行樣式
        using (var range = worksheet.Cells[1, 1, 1, 2])
        {
          range.Style.Font.Bold = true;
          range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
          range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
          range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        }

        // 填充資料
        for (int i = 0; i < data.Data.Count; i++)
        {
          var row = i + 2;
          var powerData = data.Data[i];

          worksheet.Cells[row, 1].Value = powerData.Time.ToString("yyyy-MM-dd HH:mm:ss");

          // 根據地區設置對應的用電量
          switch (region.ToLower())
          {
            case "east":
              worksheet.Cells[row, 2].Value = powerData.EastConsumption;
              break;
            case "central":
              worksheet.Cells[row, 2].Value = powerData.CentralConsumption;
              break;
            case "north":
              worksheet.Cells[row, 2].Value = powerData.NorthConsumption;
              break;
            case "south":
              worksheet.Cells[row, 2].Value = powerData.SouthConsumption;
              break;
          }

          // 設置數字格式
          worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        }

        // 自動調整欄寬
        worksheet.Cells.AutoFitColumns();

        // 設置時間欄位的格式
        worksheet.Column(1).Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

        // 添加邊框
        using (var range = worksheet.Cells[1, 1, data.Data.Count + 1, 2])
        {
          range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
          range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
          range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
          range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

        // 凍結標題行
        worksheet.View.FreezePanes(2, 1);

        return package.GetAsByteArray();
      }
    }

    /// <summary>
    /// 取得地區中文名稱
    /// </summary>
    /// <param name="region">地區英文代碼</param>
    /// <returns>地區中文名稱</returns>
    private string GetRegionName(string region)
    {
      return region.ToLower() switch
      {
        "east" => "東部",
        "central" => "中部",
        "north" => "北部",
        "south" => "南部",
        _ => region
      };
    }
  }
}