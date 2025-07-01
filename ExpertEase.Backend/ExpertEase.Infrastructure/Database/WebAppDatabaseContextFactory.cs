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
            "Host=db.xkwlftmopgzgynxblfgh.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=qzaAUP7Uz&zyJhL;";
        optionsBuilder.UseNpgsql(connectionString);

        return new WebAppDatabaseContext(optionsBuilder.Options, migrate: false); // avoid auto-migrating during CLI commands
    }
}