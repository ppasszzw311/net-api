namespace NET_API.Services.LineBot.Strategies
{
    public interface IMessageStrategy
    {
        Task<string> ExecuteAsync(string message);
    }
}
