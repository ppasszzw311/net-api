using System.Security.Cryptography.X509Certificates;
using static NET_API.Dtos.MessageEventDto;

namespace NET_API.Dtos;

public class WebhookEventsDto
{
    public string Type { get; set; } // 事件類型
    public string Mode { get; set; } // Channel state : active | standby
    public long Timestamp { get; set; } // 事件發生時間 : event occurred time in milliseconds
    public SourceDto Source { get; set; } // 事件來源 : user | group chat | multi-person chat
    public string WebhookEventId { get; set; } // webhook event id - ULID format
    public DeliverycontextDto DeliveryContext { get; set; } // 是否為重新傳送之事件 DeliveryContext.IsRedelivery : true | false

    public string? ReplyToken { get; set; } // 回覆 token
    public MessageEventDto? Message { get; set; } // 訊息事件

    public UnsendEventObjectDto? Unsend { get; set; } //使用者"收回"訊息事件
    
}

public class SourceDto
{
    public string Type { get; set; } // 事件來源類型
    public string? UserId { get; set; }
    public string? GroupId { get; set; }
    public string? RoomId { get; set; }
}
public class DeliverycontextDto
{
    public bool IsRedelivery { get; set; }
}

// 事件的訊息結構
public class MessageEventDto
{
    public string Id { get; set; } // message id
    public string Type { get; set; } // message type
    // 文字訊息的狀態
    public string? Text { get; set; } // message text
    public List<TextMessageEventEmojiDto>? Emojis { get; set; } // message text
    public TextMessageEventMentionDto? Mention { get; set; } // message text
    public ContentProviderDto? ContentProvider { get; set; }
    public ImageMessageEventImageSetDto? ImageSet { get; set; }
    public int? Duration { get; set; } // 影片 or 音檔時長(單位：豪秒)
    //File Message Event
    public string? FileName { get; set; }
    public int? FileSize { get; set; }
    //Location Message Event
    public string? Title { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
public class TextMessageEventEmojiDto
{
    public int Index { get; set; }
    public int Length { get; set; }
    public string ProductId { get; set; }
    public string EmojiId { get; set; }
}
public class TextMessageEventMentionDto
{
    public List<TextMessageEventMentioneeDto> Mentionees { get; set; }
}

public class TextMessageEventMentioneeDto
{
    public int Index { get; set; }
    public int Length { get; set; }
    public string UserId { get; set; }
}

public class ContentProviderDto
{
    public string Type { get; set; }
    public string? OriginalContentUrl { get; set; }
    public string? PreviewImageUrl { get; set; }
}

public class ImageMessageEventImageSetDto
{
    public string Id { get; set; }
    public string Index { get; set; }
    public string Total { get; set; }
}

public class UnsendEventObjectDto
{
    public string messageId { get; set; }
}