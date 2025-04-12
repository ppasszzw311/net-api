using Microsoft.AspNetCore.Mvc;
using NET_API.Dtos;
using System.Text.Json;
using NET_API.Config;

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
            req.Events.ForEach(item =>
            {
                string replyToken = item.ReplyToken;
                LineBotResponseMessage message = new LineBotResponseMessage();
                message.type = "text";
                message.text = "你好三味線";
                messages.Add(message);
                result.replyToken = replyToken;
                result.messages = messages;
            });
            Console.WriteLine(result);
            Console.WriteLine(_lineBotConfig.ChannelAccessToken);
            Console.WriteLine(_lineBotConfig.ChannelSecret);
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
}