using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class ExecutionResultRepository
{
    private readonly WebIdeDbContext _db;
    public ExecutionResultRepository(WebIdeDbContext db) => _db = db;

    public ExecutionResult? GetById(int id) =>
        _db.ExecutionResults
            .FirstOrDefault(er => er.Id == id);

    public ExecutionResult? GetBySubmissionId(int submissionId) =>
        _db.ExecutionResults
            .FirstOrDefault(er => _db.Submissions
                .Any(s => s.ExecutionResultId == er.Id && s.Id == submissionId));

    public void Add(ExecutionResult er) { _db.ExecutionResults.Add(er); _db.SaveChanges(); }
}
