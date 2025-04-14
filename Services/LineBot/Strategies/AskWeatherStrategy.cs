namespace NET_API.Services.LineBot.Strategies
{
    public class AskWeatherStrategy : IMessageStrategy
    {
        public Task<string> ExecuteAsync(string message)
        {
            // 這裡可以加入你的邏輯來處理天氣查詢
            // 例如，根據用戶的輸入查詢天氣資料
            return Task.FromResult("這是天氣查詢的回應");
        }
    }
}
