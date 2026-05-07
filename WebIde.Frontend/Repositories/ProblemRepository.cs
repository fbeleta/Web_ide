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
            .Include(p => p.Tags)
            .Include(p => p.Submissions)
            .OrderBy(p => p.Id)
            .ToList();

    public Problem? GetById(int id) =>
        _db.Problems
            .Include(p => p.Tags)
            .Include(p => p.TestCases)
            .Include(p => p.Submissions).ThenInclude(s => s.User)
            .FirstOrDefault(p => p.Id == id);
}
