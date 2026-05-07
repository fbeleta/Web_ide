using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class UserRepository
{
    private readonly WebIdeDbContext _db;
    public UserRepository(WebIdeDbContext db) => _db = db;

    public List<User> GetAll() =>
        _db.Users
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations)
            .OrderBy(u => u.Id)
            .ToList();

    public User? GetById(int id) =>
        _db.Users
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations).ThenInclude(o => o.ProblemSets)
            .FirstOrDefault(u => u.Id == id);
}
