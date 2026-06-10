using System.ComponentModel.DataAnnotations;

namespace WebIde.Web.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime RegisteredAt { get; set; }
}

public class CreateUserDto
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = "";

    public string Role { get; set; } = "Student";
}

public class UpdateUserDto
{
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Role { get; set; }
}
