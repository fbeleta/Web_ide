using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class ProblemSetRepository
{
    private readonly WebIdeDbContext _db;
    public ProblemSetRepository(WebIdeDbContext db) => _db = db;

    public List<ProblemSet> GetAll() =>
        _db.ProblemSets
            .Where(ps => ps.DeletedAt == null)
            .Include(ps => ps.Organization)
            .Include(ps => ps.Problems)
            .OrderBy(ps => ps.OrderIndex)
            .ToList();

    public ProblemSet? GetById(int id) =>
        _db.ProblemSets
            .Where(ps => ps.DeletedAt == null)
            .Include(ps => ps.Organization)
            .Include(ps => ps.Problems).ThenInclude(p => p.Tags)
            .Include(ps => ps.Problems).ThenInclude(p => p.Submissions)
            .FirstOrDefault(ps => ps.Id == id);

    public List<ProblemSet> Search(string q) =>
        _db.ProblemSets
            .Where(ps => ps.DeletedAt == null && ps.Title.ToLower().Contains(q.ToLower()))
            .Include(ps => ps.Organization)
            .OrderBy(ps => ps.Title)
            .Take(20)
            .ToList();

    public void Add(ProblemSet ps) { _db.ProblemSets.Add(ps); _db.SaveChanges(); }

    public void Update() => _db.SaveChanges();

    public void SoftDelete(int id)
    {
        var ps = _db.ProblemSets.Find(id);
        if (ps != null) { ps.DeletedAt = DateTime.UtcNow; _db.SaveChanges(); }
    }
}
