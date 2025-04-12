namespace NET_API.Dtos;

public class WebhookRequestBody
{
    public string? Destination { get; set; }
    public List<WebhookEventsDto> Events { get; set; }
}