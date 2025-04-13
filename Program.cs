using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using NET_API.Config;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;

var builder = WebApplication.CreateBuilder(args);

// 在開發環境加載 .env 文件
if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

// 配置日誌
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 註冊 LineBotConfig
builder.Services.AddSingleton<LineBotConfig>();
builder.Services.AddSingleton<DbConnConfig>();

// 註冊LineBot
builder.Services.AddHttpClient("LineBot", (serviceProvider, client) => {
    var config = serviceProvider.GetRequiredService<LineBotConfig>();
    client.BaseAddress = new Uri("https://api.line.me/v2/bot/message/reply");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ChannelAccessToken}");
});

// 添加資料庫上下文服務
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    var dbConnConfig = new DbConnConfig();
    options.UseNpgsql(dbConnConfig.DefaultConnection, npgsqlOptions => {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null
        );
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用 CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

