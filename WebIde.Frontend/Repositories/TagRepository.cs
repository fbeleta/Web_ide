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
            .Where(t => t.DeletedAt == null)
            .Include(t => t.Problems)
            .OrderBy(t => t.Id)
            .ToList();

    public Tag? GetById(int id) =>
        _db.Tags
            .Where(t => t.DeletedAt == null)
            .Include(t => t.Problems).ThenInclude(p => p.Tags)
            .Include(t => t.Problems).ThenInclude(p => p.Submissions)
            .FirstOrDefault(t => t.Id == id);

    public List<Tag> Search(string q) =>
        _db.Tags
            .Where(t => t.DeletedAt == null && t.Name.ToLower().Contains(q.ToLower()))
            .OrderBy(t => t.Name)
            .Take(20)
            .ToList();

    public void Add(Tag tag) { _db.Tags.Add(tag); _db.SaveChanges(); }

    public void Update() => _db.SaveChanges();

    public void SoftDelete(int id)
    {
        var tag = _db.Tags.Find(id);
        if (tag != null) { tag.DeletedAt = DateTime.UtcNow; _db.SaveChanges(); }
    }
}
