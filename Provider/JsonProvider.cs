using System.Text.Json;

namespace LineBot.Providers;

public class JsonProvider
{
    private JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}