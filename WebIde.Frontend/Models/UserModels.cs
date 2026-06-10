using System.ComponentModel.DataAnnotations;
using WebIde.Model.Enums;

namespace WebIde.Frontend.Models;

public class UserCreateModel
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Username may only contain letters, digits, underscores and hyphens.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(200)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Display name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Display name must be between 1 and 100 characters.")]
    public string DisplayName { get; set; } = "";

    public UserRole Role { get; set; } = UserRole.Student;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public class UserEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Username may only contain letters, digits, underscores and hyphens.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(200)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Display name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Display name must be between 1 and 100 characters.")]
    public string DisplayName { get; set; } = "";

    public UserRole Role { get; set; }

    public DateTime RegisteredAt { get; set; }
}
