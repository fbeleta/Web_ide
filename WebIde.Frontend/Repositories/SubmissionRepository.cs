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

    public Task<bool> IsOwnedByAsync(int submissionId, int userId) =>
        _db.Submissions.AnyAsync(s => s.Id == submissionId && s.UserId == userId);

    public async Task<Submission> CreateAsync(int userId, int problemId, string language, string sourceCode)
    {
        var submission = new Submission
        {
            UserId      = userId,
            ProblemId   = problemId,
            Language    = language,
            SourceCode  = sourceCode,
            Status      = WebIde.Model.Enums.SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
        };
        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();
        return submission;
    }
}
