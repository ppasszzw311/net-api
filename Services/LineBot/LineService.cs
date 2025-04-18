


using System.Text;
using System.Text.Json;

namespace NET_API.Services.LineBot;

public class LineService
{
  private readonly IHttpClientFactory _httpClientFactory;

  public LineService(IHttpClientFactory httpClientFactory)
  {
    _httpClientFactory = httpClientFactory;
  }

  public async Task SendPushMessage(string toUserId, string message)
  {
    var client = _httpClientFactory.CreateClient("LineBot");

    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.line.me/v2/bot/message/push");

    var payload = new
    { 
      to = toUserId,
      message = new[]
      {
        new { type = "text", text = message }
      }
    };

    string json = JsonSerializer.Serialize(payload);

    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.SendAsync(request);

    response.EnsureSuccessStatusCode();
  }
}