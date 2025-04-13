namespace NET_API.Config;

public class DbConnConfig
{
    public string DefaultConnection { get; set; }

    public DbConnConfig()
    {
        DefaultConnection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
            ?? throw new ArgumentNullException(nameof(DefaultConnection), "DB_CONNECTION_STRING is not set");
    }
}