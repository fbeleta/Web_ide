using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class TestCaseRepository
{
    private readonly WebIdeDbContext _db;
    public TestCaseRepository(WebIdeDbContext db) => _db = db;

    public List<TestCase> GetByProblemId(int problemId) =>
        _db.TestCases
            .Where(tc => tc.ProblemId == problemId && tc.DeletedAt == null)
            .OrderBy(tc => tc.OrderIndex)
            .ToList();

    public TestCase? GetById(int id) =>
        _db.TestCases
            .Include(tc => tc.Problem)
            .FirstOrDefault(tc => tc.Id == id && tc.DeletedAt == null);

    public List<TestCase> Search(int problemId, string q) =>
        _db.TestCases
            .Where(tc => tc.ProblemId == problemId && tc.DeletedAt == null &&
                (tc.InputArgs.ToLower().Contains(q.ToLower()) || tc.ExpectedOutput.ToLower().Contains(q.ToLower())))
            .OrderBy(tc => tc.OrderIndex)
            .Take(20)
            .ToList();

    public void Add(TestCase tc) { _db.TestCases.Add(tc); _db.SaveChanges(); }

    public void Update() => _db.SaveChanges();

    public void SoftDelete(int id)
    {
        var tc = _db.TestCases.Find(id);
        if (tc != null) { tc.DeletedAt = DateTime.UtcNow; _db.SaveChanges(); }
    }
}
