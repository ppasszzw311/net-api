
namespace NET_API.Services.LineBot.Strategies
{
    public class ReplyTextStrategy : IMessageStrategy
    {
        public Task<string> ExecuteAsync(string message)
        {
            return Task.FromResult("回應你的內容");
        }
    }
}
