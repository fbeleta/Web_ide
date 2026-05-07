using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class ProblemRepository(WebIdeDbContext db)
{
    public List<Problem> GetAll() => db.Problems
        .Include(p => p.Tags)
        .OrderBy(p => p.Id)
        .ToList();

    public Problem? GetById(int id) => db.Problems
        .Include(p => p.Tags)
        .Include(p => p.TestCases)
        .Include(p => p.Submissions).ThenInclude(s => s.User)
        .FirstOrDefault(p => p.Id == id);
}
