using System.Text;
using System.Text.Json;
using NET_API.Dtos.TaiPower;

namespace NET_API.Services
{
    public class TaiPowerChartService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TaiPowerChartService> _logger;
        private const string API_BASE_URL = "https://mak-draw-api.zeabur.app";

        public TaiPowerChartService(HttpClient httpClient, ILogger<TaiPowerChartService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// 生成PNG格式的台電用電量圖表
        /// </summary>
        /// <param name="powerData">台電用電量數據</param>
        /// <returns>PNG圖表的字節數組</returns>
        public async Task<byte[]> GenerateChartPngAsync(List<PowerData> powerData)
        {
            try
            {
                var requestData = new
                {
                    data = powerData.Select(p => new
                    {
                        time = p.Time.ToString("yyyy-MM-ddTHH:mm:ss"),
                        eastConsumption = p.EastConsumption ?? 0,
                        centralConsumption = p.CentralConsumption ?? 0,
                        northConsumption = p.NorthConsumption ?? 0,
                        southConsumption = p.SouthConsumption ?? 0
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("正在調用Draw API生成PNG圖表，數據點數量: {DataCount}", powerData.Count);

                var response = await _httpClient.PostAsync($"{API_BASE_URL}/generate-chart", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var pngBytes = await response.Content.ReadAsByteArrayAsync();
                    _logger.LogInformation("PNG圖表生成成功，大小: {Size} bytes", pngBytes.Length);
                    return pngBytes;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Draw API返回錯誤: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Draw API錯誤: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成PNG圖表時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 生成Base64格式的台電用電量圖表
        /// </summary>
        /// <param name="powerData">台電用電量數據</param>
        /// <returns>包含Base64圖表數據的響應</returns>
        public async Task<ChartBase64Response> GenerateChartBase64Async(List<PowerData> powerData)
        {
            try
            {
                var requestData = new
                {
                    data = powerData.Select(p => new
                    {
                        time = p.Time.ToString("yyyy-MM-ddTHH:mm:ss"),
                        eastConsumption = p.EastConsumption ?? 0,
                        centralConsumption = p.CentralConsumption ?? 0,
                        northConsumption = p.NorthConsumption ?? 0,
                        southConsumption = p.SouthConsumption ?? 0
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("正在調用Draw API生成Base64圖表，數據點數量: {DataCount}", powerData.Count);

                var response = await _httpClient.PostAsync($"{API_BASE_URL}/generate-chart-base64", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var chartResponse = JsonSerializer.Deserialize<ChartBase64Response>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    _logger.LogInformation("Base64圖表生成成功");
                    return chartResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Draw API返回錯誤: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Draw API錯誤: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成Base64圖表時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 檢查Draw API服務健康狀態
        /// </summary>
        /// <returns>服務是否可用</returns>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{API_BASE_URL}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查Draw API健康狀態時發生錯誤");
                return false;
            }
        }
    }

    public class ChartBase64Response
    {
        public string ChartBase64 { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
} 