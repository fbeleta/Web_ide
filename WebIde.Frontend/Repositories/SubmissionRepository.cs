using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class SubmissionRepository(WebIdeDbContext db)
{
    public List<Submission> GetAll() => db.Submissions
        .Include(s => s.User)
        .Include(s => s.Problem)
        .OrderByDescending(s => s.SubmittedAt)
        .ToList();

    public Submission? GetById(int id) => db.Submissions
        .Include(s => s.User)
        .Include(s => s.Problem)
        .Include(s => s.ExecutionResult)
        .FirstOrDefault(s => s.Id == id);

    public List<Submission> GetByProblem(int problemId) => db.Submissions
        .Include(s => s.User)
        .Where(s => s.ProblemId == problemId)
        .OrderByDescending(s => s.SubmittedAt)
        .ToList();
}
