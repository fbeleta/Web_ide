using System.ComponentModel.DataAnnotations;

namespace WebIde.Web.DTOs;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class CreateTagDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
}

public class UpdateTagDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
}
