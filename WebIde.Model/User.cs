using WebIde.Model.Enums;

namespace WebIde.Model;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public UserRole Role { get; set; }
    public DateTime RegisteredAt { get; set; }
    public List<Submission> Submissions { get; set; }
    public List<Organization> Organizations { get; set; }

    public User()
    {
        Submissions = new List<Submission>();
        Organizations = new List<Organization>();
    }
}
