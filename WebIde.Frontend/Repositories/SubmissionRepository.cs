using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class SubmissionRepository
{
    private readonly WebIdeDbContext _db;
    public SubmissionRepository(WebIdeDbContext db) => _db = db;

    public List<Submission> GetAll() =>
        _db.Submissions
            .Include(s => s.User)
            .Include(s => s.Problem)
            .Include(s => s.ExecutionResult)
            .OrderByDescending(s => s.SubmittedAt)
            .ToList();

    public Submission? GetById(int id) =>
        _db.Submissions
            .Include(s => s.User)
            .Include(s => s.Problem)
            .Include(s => s.ExecutionResult)
            .FirstOrDefault(s => s.Id == id);
}
