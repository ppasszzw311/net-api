using Microsoft.AspNetCore.Mvc;
using NET_API.Dtos;
using System.Text.Json;

namespace NET_API.Controllers;

[ApiController]
[Route("[controller]")]
public class LineBotController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    // 建立channel secret與channel access token
    private readonly string _channelAccessToken; 
    private readonly string _channelSecret;

    public LineBotController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _channelAccessToken = configuration["LineBot:ChannelAccessToken"] ?? throw new ArgumentNullException(nameof(_channelAccessToken));
        _channelSecret = configuration["LineBot:ChannelSecret"] ?? throw new ArgumentNullException(nameof(_channelSecret));
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