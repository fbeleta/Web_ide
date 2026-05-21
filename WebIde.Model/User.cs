using System.ComponentModel.DataAnnotations;
using WebIde.Model.Enums;

namespace WebIde.Model;

public class User
{
    [Key]
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public UserRole Role { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
