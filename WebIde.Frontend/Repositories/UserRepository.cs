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
            .Where(u => u.DeletedAt == null)
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations)
            .OrderBy(u => u.Id)
            .ToList();

    public User? GetById(int id) =>
        _db.Users
            .Where(u => u.DeletedAt == null)
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations).ThenInclude(o => o.ProblemSets)
            .FirstOrDefault(u => u.Id == id);

    public List<User> Search(string q) =>
        _db.Users
            .Where(u => u.DeletedAt == null &&
                (u.Username.ToLower().Contains(q.ToLower()) || u.DisplayName.ToLower().Contains(q.ToLower())))
            .OrderBy(u => u.Username)
            .Take(20)
            .ToList();

    public void Add(User user) { _db.Users.Add(user); _db.SaveChanges(); }

    public void Update() => _db.SaveChanges();

    public void SoftDelete(int id)
    {
        var user = _db.Users.Find(id);
        if (user != null) { user.DeletedAt = DateTime.UtcNow; _db.SaveChanges(); }
    }
}
