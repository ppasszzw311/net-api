using Microsoft.EntityFrameworkCore;
using NET_API.Models;
using NET_API.Models.Stock;

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

    // 使用者相關
    // 使用者
    public DbSet<UserModel> Users {get; set;}
    // 使用者權限
    public DbSet<RoleModel> Roles {get; set;}
    // 使用者有的權限
    public DbSet<UserHasRole> UserHasRoles {get; set;}

    // Stock
    public DbSet<StockData> StockDatas {get; set;}
} 