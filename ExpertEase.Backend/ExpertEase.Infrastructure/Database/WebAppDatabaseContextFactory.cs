using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace ExpertEase.Infrastructure.Database;

public class WebAppDatabaseContextFactory : IDesignTimeDbContextFactory<WebAppDatabaseContext>
{
    public WebAppDatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WebAppDatabaseContext>();

        // Replace this with your actual connection string
        var connectionString =
            "Host=34.78.213.177;Port=5432;Database=experteasedb;Username=postgres;Password=mypassword;";
        optionsBuilder.UseNpgsql(connectionString);

        return new WebAppDatabaseContext(optionsBuilder.Options, migrate: false); // avoid auto-migrating during CLI commands
    }
}