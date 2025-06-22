using NET_API.Dtos.TaiPower;
using System.IO;

namespace NET_API.Services
{
  public class ChartService
  {
    private readonly FontService _fontService;

    public ChartService(FontService fontService)
    {
      _fontService = fontService;
    }

    /// <summary>
    /// 生成台電用電量折線圖並轉換為 PNG 格式
    /// 注意：此服務已停用，請使用 HtmlChartService
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <returns>PNG 圖片的 byte 陣列</returns>
    public byte[] GenerateTaiPowerLineChartAsPng(PowerDataResponse data)
    {
      Console.WriteLine("ChartService 已停用，請使用 HtmlChartService");
      return Array.Empty<byte>();
    }
  }
}