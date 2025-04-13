using Microsoft.AspNetCore.Mvc;
using NET_API.Dtos;
using System.Text.Json;
using NET_API.Config;
using System.Threading.Tasks;
using NET_API.Models.CWB;

namespace NET_API.Controllers;

[ApiController]
[Route("[controller]")]
public class LineBotController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LineBotConfig _lineBotConfig;

    public LineBotController(IHttpClientFactory httpClientFactory, LineBotConfig lineBotConfig)
    {
        _httpClientFactory = httpClientFactory;
        _lineBotConfig = lineBotConfig;
    }

    [HttpPost]
    public async Task<IActionResult> Post(WebhookRequestBody req)
    {
        LineBotResponse result = new LineBotResponse();
        List<LineBotResponseMessage> messages = new List<LineBotResponseMessage>();

        if (req.Events.Count > 0)
        {
            foreach(var item in req.Events)
            {
                if (item.Type != "message")
                {
                    // 跳過不執行
                    continue;
                }
                string replyToken = item.ReplyToken;
                string requestText = item.Message.Text;

                LineBotResponseMessage message = await GetResponse(requestText);
                messages.Add(message);
                result.replyToken = replyToken;
                result.messages = messages;
                Console.WriteLine(message.text);
            };
            // 呼叫reply api
            var client = _httpClientFactory.CreateClient("LineBot");
            var response = await client.PostAsJsonAsync("", result);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }
        }
        return Ok(result);
    }

    // 判斷是否是要進行天氣的判斷
    private async Task<LineBotResponseMessage> GetResponse(string reqText)
    {
        LineBotResponseMessage message = new LineBotResponseMessage();

        if (reqText.Contains("天氣"))
        {
            // 如果是天氣就呼叫天氣
            var city = ParseCity(reqText);

            // 呼叫中央氣象局api
            var weatherData = await GetWeatherFromCWB(city);

            message.type = "text";
            message.text = weatherData;

        }
        else
        {
            message.type = "text";
            message.text = "你好三味線";
        }

        return message;
    }

    // 判斷解析天氣
    private string? ParseCity(string message)
    {
        if (message.Contains("台北")) return "臺北市";
        if (message.Contains("台中")) return "臺中市";
        if (message.Contains("高雄")) return "高雄是";

        return null;
    }

    // 呼叫中央氣象局api
    private async Task<string> GetWeatherFromCWB(string city)
    {
        var apiKey = "CWA-80BFADB3-4CFC-4E82-AC81-77022337F2A8";
        var url = $"https://opendata.cwa.gov.tw/api/v1/rest/datastore/F-C0032-001?Authorization={apiKey}&locationName={city}";

        using var client = new HttpClient();
        string response = await client.GetStringAsync(url);

        return response;
    }
}