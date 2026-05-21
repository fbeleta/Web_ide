using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class OrganizationRepository
{
    private readonly WebIdeDbContext _db;
    public OrganizationRepository(WebIdeDbContext db) => _db = db;

    public List<Organization> GetAll() =>
        _db.Organizations
            .Where(o => o.DeletedAt == null)
            .Include(o => o.Members)
            .Include(o => o.ProblemSets)
            .OrderBy(o => o.Id)
            .ToList();

    public Organization? GetById(int id) =>
        _db.Organizations
            .Where(o => o.DeletedAt == null)
            .Include(o => o.Members)
            .Include(o => o.ProblemSets).ThenInclude(ps => ps.Problems).ThenInclude(p => p.Tags)
            .FirstOrDefault(o => o.Id == id);

    public List<Organization> Search(string q) =>
        _db.Organizations
            .Where(o => o.DeletedAt == null && o.Name.ToLower().Contains(q.ToLower()))
            .OrderBy(o => o.Name)
            .Take(20)
            .ToList();

    public void Add(Organization org) { _db.Organizations.Add(org); _db.SaveChanges(); }

    public void Update() => _db.SaveChanges();

    public void SoftDelete(int id)
    {
        var org = _db.Organizations.Find(id);
        if (org != null) { org.DeletedAt = DateTime.UtcNow; _db.SaveChanges(); }
    }
}
