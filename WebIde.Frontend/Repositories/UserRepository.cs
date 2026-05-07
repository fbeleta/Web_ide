using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;

namespace WebIde.Web.Repositories;

public class UserRepository(WebIdeDbContext db)
{
    public List<User> GetAll() => db.Users
        .OrderBy(u => u.Id)
        .ToList();

    public User? GetById(int id) => db.Users
        .Include(u => u.Submissions).ThenInclude(s => s.Problem)
        .Include(u => u.Organizations)
        .FirstOrDefault(u => u.Id == id);
}
