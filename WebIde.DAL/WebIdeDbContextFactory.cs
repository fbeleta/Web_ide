using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebIde.DAL;

public class WebIdeDbContextFactory : IDesignTimeDbContextFactory<WebIdeDbContext>
{
    public WebIdeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WebIdeDbContext>()
            .UseNpgsql(
                "Host=postgres;Port=5432;Database=webide;Username=webide;Password=ch2excrkZC7UgO5VMXINuTVT5UEh/lbS7bu/aLE5JnI=",
                o => o.MigrationsAssembly("WebIde.DAL"))
            .Options;

        return new WebIdeDbContext(options);
    }
}
