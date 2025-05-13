using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using NET_API.Config;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;
using NET_API.Services.LineBot;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using NET_API.Services.SignalR;

var builder = WebApplication.CreateBuilder(args);

// 取得環境
var environment = builder.Environment;

// 在開發環境加載 .env 文件
if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

// 配置數據保護
builder.Services.AddDataProtection()
    .SetApplicationName("NugAPI");

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
builder.Services.AddSignalR(); // 加入SignalR支援
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 JWT 認證（但不強制要求）
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// 註冊 LineBotConfig
builder.Services.AddSingleton<LineBotConfig>();
builder.Services.AddSingleton<DbConnConfig>();

// 註冊LineBot
builder.Services.AddHttpClient("LineBot", (serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<LineBotConfig>();
    client.BaseAddress = new Uri("https://api.line.me/v2/bot/message/reply");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ChannelAccessToken}");
});

builder.Services.AddScoped<LineService>();

// 添加資料庫上下文服務
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var dbConnConfig = new DbConnConfig();
    builder.Configuration.GetSection("Database").Bind(dbConnConfig);

    // 判定環境取得不同資料庫
    if (environment.IsDevelopment())
    {
        options.UseSqlite(dbConnConfig.GetConnectionString());
    }
    else
    {
        options.UseNpgsql(dbConnConfig.GetConnectionString(), npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
        });
    }
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

// 添加認證和授權中間件（但不強制要求）
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/charhub"); // 註冊路由

app.Run();

