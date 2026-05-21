using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class ProblemRepository
{
    private readonly WebIdeDbContext _db;
    public ProblemRepository(WebIdeDbContext db) => _db = db;

    public List<Problem> GetAll() =>
        _db.Problems
            .Where(p => p.DeletedAt == null)
            .Include(p => p.Tags)
            .Include(p => p.Submissions)
            .OrderBy(p => p.Id)
            .ToList();

    public Problem? GetById(int id) =>
        _db.Problems
            .Where(p => p.DeletedAt == null)
            .Include(p => p.Tags)
            .Include(p => p.TestCases)
            .Include(p => p.Submissions).ThenInclude(s => s.User)
            .FirstOrDefault(p => p.Id == id);

    public List<Problem> Search(string q) =>
        _db.Problems
            .Where(p => p.DeletedAt == null && p.Title.ToLower().Contains(q.ToLower()))
            .Include(p => p.Tags)
            .OrderBy(p => p.Title)
            .Take(20)
            .ToList();

    public void Add(Problem problem) { _db.Problems.Add(problem); _db.SaveChanges(); }

    public void Update() => _db.SaveChanges();

    public void SoftDelete(int id)
    {
        var problem = _db.Problems.Find(id);
        if (problem != null) { problem.DeletedAt = DateTime.UtcNow; _db.SaveChanges(); }
    }
}
