namespace NET_API.Dtos;

public class LineBotResponse
{
    public string replyToken {get; set;}
    public List<LineBotResponseMessage> messages {get; set;}
}
public class LineBotResponseMessage 
{
    public string type {get; set;}
    public string text {get; set;}

}