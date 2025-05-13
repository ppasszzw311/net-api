namespace NET_API.Config;

public class DbConnConfig
{
    // sqlite3設定
    public bool? UseSqlite { get; set; }
    public string? SqlitePath { get; set; }
    // postgreSQL設定
    public string Host { get; set; } = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
    public int Port { get; set; } = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432");
    public string Database { get; set; } = Environment.GetEnvironmentVariable("DB_NAME") ?? "net_api";
    public string Username { get; set; } = Environment.GetEnvironmentVariable("DB_USER") ?? "net_api_user";
    public string Password { get; set; } = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "net_api_password";

    public string GetConnectionString()
    {
        if (UseSqlite == true)
        {
            if (string.IsNullOrWhiteSpace(Password))
                throw new InvalidOperationException("請確認你的sqlitePath是否有路徑!");
            return $"Data Source={SqlitePath}";
        }
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
    }
}