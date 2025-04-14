using NET_API.Services.LineBot.Strategies;

namespace NET_API.Services.LineBot
{

    public class MessageStrategyContext
    {
        private readonly Dictionary<string, IMessageStrategy> _strategies;

        public MessageStrategyContext()
        {
            _strategies = new Dictionary<string, IMessageStrategy>
            {
                { "精打細算", new AskWeatherStrategy() },
                { "記事本", new ReplyTextStrategy() }
            };
        }

        public async Task<string> HandleMessageAsync(string message)
        {
            if (_strategies.TryGetValue(message.Trim(), out var strategy))
            {
                return await strategy.ExecuteAsync(message);
            }

            return "抱歉，我不太懂您的意思。";
        }
    }
}
