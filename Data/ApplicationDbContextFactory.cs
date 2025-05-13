using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NET_API.Config;

namespace NET_API.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // 設定base path為專案目錄
            var configutation = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();

            var dbConfig = new DbConnConfig();
            configutation.GetSection("Database").Bind(dbConfig);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (dbConfig.UseSqlite == true)
            {
                optionsBuilder.UseSqlite(dbConfig.GetConnectionString());
            }
            else
            {
                optionsBuilder.UseNpgsql(dbConfig.GetConnectionString());
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
