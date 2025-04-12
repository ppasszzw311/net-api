using Microsoft.AspNetCore.Mvc;
using NET_API.Dtos;

namespace NET_API.Controllers;

[ApiController]
[Route("[controller]")]
public class LineBotController : ControllerBase
{
    // 建立channel secret與channel access token
    private readonly string channelAccessToken = "JRV0RtDXL+6m6I0plw0rRg2pEbpt58Q5hFS9ohCa5fLiH1WoaoU0XudTJ1R/PUJDvFPHI5uagOHDwID5D+NbXI9cHn5BekxOP9IKLbKizfMMfQ2SLAdSQ/mfnMhEG52FTY5/vB3g55rwdTGnjF6NLAdB04t89/1O/w1cDnyilFU=";
    private readonly string channelSecret = "2fb60e581e38866bdb8529bfbe102804";

    public LineBotController()
    {

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
                
            }
            return Ok(result);
        }

}