namespace NET_API.Dtos;

public class TextMessageDto : BaseMessageDto
{
    public TextMessageDto()
    {
        Type = MessageTypeEnum.Text;
    }

    public string Text { get; set; }
}