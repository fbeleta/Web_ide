using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class TagRepository
{
    private readonly WebIdeDbContext _db;
    public TagRepository(WebIdeDbContext db) => _db = db;

    public List<Tag> GetAll() =>
        _db.Tags
            .Include(t => t.Problems)
            .OrderBy(t => t.Id)
            .ToList();

    public Tag? GetById(int id) =>
        _db.Tags
            .Include(t => t.Problems).ThenInclude(p => p.Tags)
            .Include(t => t.Problems).ThenInclude(p => p.Submissions)
            .FirstOrDefault(t => t.Id == id);
}
