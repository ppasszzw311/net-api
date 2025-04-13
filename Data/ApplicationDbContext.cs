using Microsoft.EntityFrameworkCore;
using NET_API.Models;

namespace NET_API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    // 在這裡添加您的 DbSet 屬性
    // 例如：
    // public DbSet<User> Users { get; set; }
} 