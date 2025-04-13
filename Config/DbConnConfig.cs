namespace NET_API.Config;

public class DbConnConfig
{
    public string Host { get; set; } = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
    public int Port { get; set; } = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432");
    public string Database { get; set; } = Environment.GetEnvironmentVariable("DB_NAME") ?? "net_api";
    public string Username { get; set; } = Environment.GetEnvironmentVariable("DB_USER") ?? "net_api_user";
    public string Password { get; set; } = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "net_api_password";

    public string GetConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
    }
}