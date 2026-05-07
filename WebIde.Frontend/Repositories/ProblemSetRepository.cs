using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class ProblemSetRepository(WebIdeDbContext db)
{
    public List<ProblemSet> GetAll() => db.ProblemSets
        .Include(ps => ps.Organization)
        .Include(ps => ps.Problems)
        .OrderBy(ps => ps.Id)
        .ToList();

    public ProblemSet? GetById(int id) => db.ProblemSets
        .Include(ps => ps.Organization)
        .Include(ps => ps.Problems).ThenInclude(p => p.Tags)
        .FirstOrDefault(ps => ps.Id == id);
}
