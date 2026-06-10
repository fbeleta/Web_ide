using System.ComponentModel.DataAnnotations;

namespace WebIde.Web.DTOs;

public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<int> MemberIds { get; set; } = [];
}

public class CreateOrganizationDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";
}

public class UpdateOrganizationDto
{
    [MaxLength(200)]
    public string? Name { get; set; }
    public string? Description { get; set; }
}
