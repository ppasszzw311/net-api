using NET_API.Dtos.TaiPower;
using PuppeteerSharp;
using System.Text;

namespace NET_API.Services
{
    public class HtmlChartService
    {
        private readonly ILogger<HtmlChartService> _logger;

        public HtmlChartService(ILogger<HtmlChartService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 生成台電用電量折線圖並轉換為 PNG 格式
        /// </summary>
        /// <param name="data">台電資料</param>
        /// <returns>PNG 圖片的 byte 陣列</returns>
        public async Task<byte[]> GenerateTaiPowerLineChartAsPngAsync(PowerDataResponse data)
        {
            try
            {
                if (data.Data.Count == 0)
                {
                    _logger.LogWarning("沒有資料可以生成圖表");
                    return Array.Empty<byte>();
                }

                var html = GenerateChartHtml(data);
                return await ConvertHtmlToImageAsync(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成圖表時發生錯誤");
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// 生成圖表的 HTML
        /// </summary>
        /// <param name="data">台電資料</param>
        /// <returns>HTML 字串</returns>
        private string GenerateChartHtml(PowerDataResponse data)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { margin: 0; padding: 20px; font-family: Arial, sans-serif; background: white; }");
            sb.AppendLine("canvas { max-width: 800px; max-height: 400px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<canvas id='chart' width='800' height='400'></canvas>");
            sb.AppendLine("<script>");

            // 準備資料
            var labels = data.Data.Select(d => d.Time.ToString("MM/dd HH:mm")).ToList();
            var eastConsumption = data.Data.Select(d => d.EastConsumption).ToList();
            var centralConsumption = data.Data.Select(d => d.CentralConsumption).ToList();
            var southConsumption = data.Data.Select(d => d.SouthConsumption).ToList();
            var northConsumption = data.Data.Select(d => d.NorthConsumption).ToList();

            sb.AppendLine("const ctx = document.getElementById('chart').getContext('2d');");
            sb.AppendLine("const chart = new Chart(ctx, {");
            sb.AppendLine("  type: 'line',");
            sb.AppendLine("  data: {");
            sb.AppendLine($"    labels: {System.Text.Json.JsonSerializer.Serialize(labels)},");
            sb.AppendLine("    datasets: [");
            sb.AppendLine("      {");
            sb.AppendLine("        label: '東部用電量',");
            sb.AppendLine($"        data: {System.Text.Json.JsonSerializer.Serialize(eastConsumption)},");
            sb.AppendLine("        borderColor: 'rgb(255, 99, 132)',");
            sb.AppendLine("        backgroundColor: 'rgba(255, 99, 132, 0.2)',");
            sb.AppendLine("        tension: 0.1");
            sb.AppendLine("      },");
            sb.AppendLine("      {");
            sb.AppendLine("        label: '中部用電量',");
            sb.AppendLine($"        data: {System.Text.Json.JsonSerializer.Serialize(centralConsumption)},");
            sb.AppendLine("        borderColor: 'rgb(54, 162, 235)',");
            sb.AppendLine("        backgroundColor: 'rgba(54, 162, 235, 0.2)',");
            sb.AppendLine("        tension: 0.1");
            sb.AppendLine("      },");
            sb.AppendLine("      {");
            sb.AppendLine("        label: '南部用電量',");
            sb.AppendLine($"        data: {System.Text.Json.JsonSerializer.Serialize(southConsumption)},");
            sb.AppendLine("        borderColor: 'rgb(255, 205, 86)',");
            sb.AppendLine("        backgroundColor: 'rgba(255, 205, 86, 0.2)',");
            sb.AppendLine("        tension: 0.1");
            sb.AppendLine("      },");
            sb.AppendLine("      {");
            sb.AppendLine("        label: '北部用電量',");
            sb.AppendLine($"        data: {System.Text.Json.JsonSerializer.Serialize(northConsumption)},");
            sb.AppendLine("        borderColor: 'rgb(75, 192, 192)',");
            sb.AppendLine("        backgroundColor: 'rgba(75, 192, 192, 0.2)',");
            sb.AppendLine("        tension: 0.1");
            sb.AppendLine("      }");
            sb.AppendLine("    ]");
            sb.AppendLine("  },");
            sb.AppendLine("  options: {");
            sb.AppendLine("    responsive: false,");
            sb.AppendLine("    plugins: {");
            sb.AppendLine("      title: {");
            sb.AppendLine("        display: true,");
            sb.AppendLine("        text: '台電用電量趨勢圖',");
            sb.AppendLine("        font: { size: 16 }");
            sb.AppendLine("      },");
            sb.AppendLine("      legend: {");
            sb.AppendLine("        display: true,");
            sb.AppendLine("        position: 'top'");
            sb.AppendLine("      }");
            sb.AppendLine("    },");
            sb.AppendLine("    scales: {");
            sb.AppendLine("      y: {");
            sb.AppendLine("        beginAtZero: true,");
            sb.AppendLine("        title: {");
            sb.AppendLine("          display: true,");
            sb.AppendLine("          text: '用電量 (萬瓩)'");
            sb.AppendLine("        }");
            sb.AppendLine("      },");
            sb.AppendLine("      x: {");
            sb.AppendLine("        title: {");
            sb.AppendLine("          display: true,");
            sb.AppendLine("          text: '時間'");
            sb.AppendLine("        }");
            sb.AppendLine("      }");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine("});");
            sb.AppendLine("</script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// 將 HTML 轉換為圖片
        /// </summary>
        /// <param name="html">HTML 內容</param>
        /// <returns>圖片的 byte 陣列</returns>
        private async Task<byte[]> ConvertHtmlToImageAsync(string html)
        {
            try
            {
                // 下載 Chromium（如果尚未下載）
                await new BrowserFetcher().DownloadAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
                });

                using var page = await browser.NewPageAsync();
                await page.SetContentAsync(html);
                
                // 等待圖表渲染完成
                await Task.Delay(3000);

                var screenshot = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Type = ScreenshotType.Png,
                    FullPage = false,
                    Clip = new PuppeteerSharp.Media.Clip
                    {
                        X = 0,
                        Y = 0,
                        Width = 840,
                        Height = 440
                    }
                });

                return screenshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "轉換 HTML 為圖片時發生錯誤");
                return Array.Empty<byte>();
            }
        }
    }
} 