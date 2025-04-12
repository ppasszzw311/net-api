namespace NET_API.Config;

public class LineBotConfig
{
    public string ChannelAccessToken { get; set; }
    public string ChannelSecret { get; set; }

    public LineBotConfig()
    {
        // 優先使用環境變數（Zeabur 環境）
        ChannelAccessToken = Environment.GetEnvironmentVariable("LINE_BOT_CHANNEL_ACCESS_TOKEN") 
            ?? throw new ArgumentNullException(nameof(ChannelAccessToken), "LINE_BOT_CHANNEL_ACCESS_TOKEN is not set");
        
        ChannelSecret = Environment.GetEnvironmentVariable("LINE_BOT_CHANNEL_SECRET") 
            ?? throw new ArgumentNullException(nameof(ChannelSecret), "LINE_BOT_CHANNEL_SECRET is not set");
    }
} 