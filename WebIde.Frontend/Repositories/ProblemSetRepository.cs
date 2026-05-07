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
            .Include(ps => ps.Organization)
            .Include(ps => ps.Problems)
            .OrderBy(ps => ps.OrderIndex)
            .ToList();

    public ProblemSet? GetById(int id) =>
        _db.ProblemSets
            .Include(ps => ps.Organization)
            .Include(ps => ps.Problems).ThenInclude(p => p.Tags)
            .Include(ps => ps.Problems).ThenInclude(p => p.Submissions)
            .FirstOrDefault(ps => ps.Id == id);
}
