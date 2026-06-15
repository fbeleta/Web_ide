using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebIde.DAL;

public class WebIdeDbContextFactory : IDesignTimeDbContextFactory<WebIdeDbContext>
{
    public WebIdeDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__WebIdeDb")
                      ?? "Host=localhost;Port=5432;Database=webide;Username=webide;Password=webide_dev";

        var options = new DbContextOptionsBuilder<WebIdeDbContext>()
            .UseNpgsql(connStr, o => o.MigrationsAssembly("WebIde.DAL"))
            .Options;

        return new WebIdeDbContext(options);
    }
}
