using Newtonsoft.Json;

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

public class FlexMessage
{
    [JsonProperty("type")]
    public string Type { get; set; } = "flex";

    [JsonProperty("altText")]
    public string AltText { get; set; }

    [JsonProperty("contents")]
    public BubbleContainer Contents { get; set; }
}

public class BubbleContainer
{
    [JsonProperty("type")]
    public string Type { get; set; } = "bubble";

    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public string Size { get; set; } // Optional (e.g., mega)

    [JsonProperty("header", NullValueHandling = NullValueHandling.Ignore)]
    public BoxComponent Header { get; set; }

    [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
    public BoxComponent Body { get; set; }
}

public class BoxComponent
{
    [JsonProperty("type")]
    public string Type { get; set; } = "box";

    [JsonProperty("layout")]
    public string Layout { get; set; } = "vertical";

    [JsonProperty("spacing", NullValueHandling = NullValueHandling.Ignore)]
    public string Spacing { get; set; }

    [JsonProperty("contents")]
    public List<TextComponent> Contents { get; set; }
}

public class TextComponent
{
    [JsonProperty("type")]
    public string Type { get; set; } = "text";

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public string Size { get; set; }

    [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
    public string Weight { get; set; }

    [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
    public string Color { get; set; }
}