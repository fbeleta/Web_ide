using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Model.Enums;

namespace WebIde.Web.Repositories;

public class UserRepository
{
    private readonly WebIdeDbContext _db;
    public UserRepository(WebIdeDbContext db) => _db = db;

    public List<User> GetAll() =>
        _db.DomainUsers
            .Where(u => u.DeletedAt == null)
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations)
            .OrderBy(u => u.Id)
            .ToList();

    public User? GetById(int id) =>
        _db.DomainUsers
            .Where(u => u.DeletedAt == null)
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations).ThenInclude(o => o.ProblemSets)
            .FirstOrDefault(u => u.Id == id);

    public List<User> Search(string q) =>
        _db.DomainUsers
            .Where(u => u.DeletedAt == null &&
                (u.Username.ToLower().Contains(q.ToLower()) || u.DisplayName.ToLower().Contains(q.ToLower())))
            .OrderBy(u => u.Username)
            .Take(20)
            .ToList();

    public void Add(User user) { _db.DomainUsers.Add(user); _db.SaveChanges(); }

    public void Update() => _db.SaveChanges();

    public void SoftDelete(int id)
    {
        var user = _db.DomainUsers.Find(id);
        if (user != null) { user.DeletedAt = DateTime.UtcNow; _db.SaveChanges(); }
    }

    public Task<User?> GetByGitHubIdAsync(string githubId) =>
        _db.DomainUsers.FirstOrDefaultAsync(u => u.GitHubId == githubId);

    public async Task<User> UpsertGitHubUserAsync(
        string githubId, string username, string displayName, string email, string avatarUrl)
    {
        var user = await _db.DomainUsers.FirstOrDefaultAsync(u => u.GitHubId == githubId);
        if (user is null)
        {
            user = new User
            {
                GitHubId     = githubId,
                Username     = username,
                DisplayName  = displayName,
                Email        = email,
                AvatarUrl    = avatarUrl,
                Role         = UserRole.Student,
                RegisteredAt = DateTime.UtcNow,
            };
            _db.DomainUsers.Add(user);
        }
        else
        {
            user.Username    = username;
            user.DisplayName = displayName;
            user.AvatarUrl   = avatarUrl;
            if (!string.IsNullOrEmpty(email)) user.Email = email;
        }
        await _db.SaveChangesAsync();
        return user;
    }

    // Maps an Identity (email/password) AppUser onto a domain User so the rest of
    // the site (navbar, submissions, hub) can key off webide:userId like GitHub users.
    public async Task<User> UpsertLocalUserAsync(string email, string username, string displayName)
    {
        var user = await _db.DomainUsers
            .FirstOrDefaultAsync(u => u.GitHubId == null && u.Email == email);
        if (user is null)
        {
            user = new User
            {
                Username     = username,
                DisplayName  = displayName,
                Email        = email,
                Role         = UserRole.Student,
                RegisteredAt = DateTime.UtcNow,
            };
            _db.DomainUsers.Add(user);
        }
        else
        {
            user.Username    = username;
            user.DisplayName = displayName;
        }
        await _db.SaveChangesAsync();
        return user;
    }
}
