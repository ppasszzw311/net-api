using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using NET_API.Config;
using Microsoft.Extensions.Logging;

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

// 註冊LineBot
builder.Services.AddHttpClient("LineBot", (serviceProvider, client) => {
    var config = serviceProvider.GetRequiredService<LineBotConfig>();
    client.BaseAddress = new Uri("https://api.line.me/v2/bot/message/reply");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ChannelAccessToken}");
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

