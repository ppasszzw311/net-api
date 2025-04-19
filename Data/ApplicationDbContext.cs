using Microsoft.EntityFrameworkCore;
using NET_API.Models;
using NET_API.Models.Nug;
using NET_API.Models.Stock;

namespace NET_API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    // 使用者相關
    // 使用者
    public DbSet<UserModel> Users { get; set; }
    // 使用者權限
    public DbSet<RoleModel> Roles { get; set; }
    // 使用者有的權限
    public DbSet<UserHasRole> UserHasRoles { get; set; }

    // Stock
    public DbSet<StockData> StockDatas { get; set; }

    // nug
    public DbSet<NugUser> NugUsers { get; set; }
    public DbSet<NugStore> NugStores { get; set; }
    public DbSet<NugProduct> NugProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NugStore>()
            .HasOne(s => s.Owner)
            .WithMany()
            .HasForeignKey(s => s.OwnerId)
            .HasPrincipalKey(u => u.Id);

        // 配置 NugProduct 和 NugStore 之間的關係
        modelBuilder.Entity<NugProduct>()
            .HasOne(p => p.Store)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StoreId)
            .HasPrincipalKey(s => s.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}