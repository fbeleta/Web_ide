using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebIde.DAL;

public class WebIdeDbContextFactory : IDesignTimeDbContextFactory<WebIdeDbContext>
{
    public WebIdeDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("WEBIDE_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=webide;Username=webide;Password=PLACEHOLDER";

        var options = new DbContextOptionsBuilder<WebIdeDbContext>()
            .UseNpgsql(connStr, o => o.MigrationsAssembly("WebIde.DAL"))
            .Options;

        return new WebIdeDbContext(options);
    }
}
