using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class TagRepository(WebIdeDbContext db)
{
    public List<Tag> GetAll() => db.Tags
        .Include(t => t.Problems)
        .OrderBy(t => t.Name)
        .ToList();

    public Tag? GetById(int id) => db.Tags
        .Include(t => t.Problems)
        .FirstOrDefault(t => t.Id == id);
}
