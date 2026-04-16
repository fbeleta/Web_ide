using WebIde.Model;

namespace WebIde.Web.Repositories;

public class UserRepository
{
    public List<User> GetAll() => MockData.Users;
    public User? GetById(int id) => MockData.Users.FirstOrDefault(u => u.Id == id);
}
