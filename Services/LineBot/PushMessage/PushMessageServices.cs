// 自動推播訊息

namespace LineBot.Services.PushMessage
{
    public class PushMessageServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LineBotConfig _lineBotConfig;

        public PushMessageServices(IHttpClientFactory httpClientFactory, LineBotConfig lineBotConfig)
        {
            _httpClientFactory = httpClientFactory;
            _lineBotConfig = lineBotConfig;
        }

        public async Task PushMessage(string userId, string message)
        {
            var client = _httpClientFactory.CreateClient();
            var requestBody = new
            {
                to = new[] { userId },
                messages = new[] { new { type = "text", text = message } }
            };
            
            var client = _httpClientFactory.CreateClient();
            var requestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_lineBotConfig.PushMessageUrl, content);
            response.EnsureSuccessStatusCode();
        }   

        // push flex message
        public async Task PushFlexMessage(string userId, FlexMessage flexMessage)
        {
            var client = _httpClientFactory.CreateClient();
            var requestBody = new
            {
                to = new[] { userId },
                messages = new[] { new { type = "flex", altText = "Flex Message", contents = flexMessage } }
            };

            var requestBodyJson = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");    

            var response = await client.PostAsync(_lineBotConfig.PushMessageUrl, content);
            response.EnsureSuccessStatusCode();
        }

        // 推送flex message 固定版型
        public async Task PushFlexMessage(string userId, string city)
        {
            var flexMessage = GetWeatherCard(city);
            await PushFlexMessage(userId, flexMessage);
        }
    }
}