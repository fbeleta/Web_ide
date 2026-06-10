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
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations)
            .OrderBy(u => u.Id)
            .ToList();

    public User? GetById(int id) =>
        _db.DomainUsers
            .Include(u => u.Submissions).ThenInclude(s => s.Problem)
            .Include(u => u.Organizations).ThenInclude(o => o.ProblemSets)
            .FirstOrDefault(u => u.Id == id);

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
}
