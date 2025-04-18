using Microsoft.AspNetCore.Mvc;
using NET_API.Dtos;
using NET_API.Config;
using NET_API.Models.CWB;
using Newtonsoft.Json;
using NET_API.Services.LineBot;

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

        // 取得user i
        //var userId = 

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

                var message = await GetResponse(requestText);

                if (message.text == "flex")
                {
                    var weatherFlex = GetWeatherCard("台北");  // 這是你的 FlexMessage物件
                    var flexJson = JsonConvert.SerializeObject(weatherFlex);

                    var payload = new
                    {
                        replyToken = replyToken,
                        messages = new object[]
                        {
                            JsonConvert.DeserializeObject(flexJson)  // 把FlexMessage放進messages array
                        }
                    };

                    var clienta = _httpClientFactory.CreateClient("LineBot");
                    var responsea = await clienta.PostAsJsonAsync("", payload);

                    return Ok(payload);
                }
                else
                {
                    messages.Add(message);
                    result.messages = messages;
                }
                
                result.replyToken = replyToken;
                
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

            if (city != null)
            {
                // 呼叫中央氣象局api
                var weatherData = await GetWeatherFromCWB(city);

                if (weatherData.Success == "true")
                {
                    // 回傳圖卡
                    //GetWeatherCard(weatherData);
                    message.type = "flex";
                    message.text = "台北是";
                }
            }
            else
            {
                message.type = "text";
                message.text = "有API";
            }
        }
        else
        {
            var strategyContext = new MessageStrategyContext();
            string replyText = await strategyContext.HandleMessageAsync(reqText);
            message.type = "text";
            message.text = replyText;
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
    private async Task<WeatherDataCWBModel> GetWeatherFromCWB(string city)
    {
        var apiKey = "CWA-80BFADB3-4CFC-4E82-AC81-77022337F2A8";
        var url = $"https://opendata.cwa.gov.tw/api/v1/rest/datastore/F-C0032-001?Authorization={apiKey}&locationName={city}";

        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);

        Console.WriteLine(response);

        var weatherData = JsonConvert.DeserializeObject<WeatherDataCWBModel>(response);

        return weatherData;
    }

    private FlexMessage GetWeatherCard(string cwbData)
    {
        FlexMessage message = new FlexMessage();

        // 取得資料
        var city = "台北市";
        var weather = "多雲短暫陣雨";
        var temperature = "22°C ~ 28°C";
        var rainProbability = "30";
        var comfort = "稍感悶熱";

        var flexMessage = new FlexMessage
        {
            AltText = "天氣資訊",
            Contents = new BubbleContainer
            {
                Size = "mega",
                Header = new BoxComponent
                {
                    Contents = new List<TextComponent>
            {
                new TextComponent
                {
                    Text = "臺北市 天氣",
                    Size = "xl",
                    Weight = "bold"
                }
            }
                },
                Body = new BoxComponent
                {
                    Spacing = "md",
                    Contents = new List<TextComponent>
            {
                new TextComponent
                {
                    Text = "☀️ 晴朗",
                    Size = "md"
                },
                new TextComponent
                {
                    Text = "🌡️ 氣溫：22°C ~ 28°C",
                    Size = "sm",
                    Color = "#555555"
                },
                new TextComponent
                {
                    Text = "💧 降雨機率：30%",
                    Size = "sm",
                    Color = "#555555"
                },
                new TextComponent
                {
                    Text = "😊 舒適度：稍感悶熱",
                    Size = "sm",
                    Color = "#555555"
                }
            }
                }
            }
        };

        var flexJson = JsonConvert.SerializeObject(flexMessage);

        return flexMessage;
    }
}