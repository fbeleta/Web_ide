using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebIde.DAL;

public class WebIdeDbContextFactory : IDesignTimeDbContextFactory<WebIdeDbContext>
{
    public WebIdeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WebIdeDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=webide;Username=webide;Password=webide_dev",
                o => o.MigrationsAssembly("WebIde.DAL"))
            .Options;

        return new WebIdeDbContext(options);
    }
}
