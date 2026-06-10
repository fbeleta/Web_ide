using System.ComponentModel.DataAnnotations;

namespace WebIde.Frontend.Models;

public class OrganizationCreateModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 2000 characters.")]
    public string Description { get; set; } = "";
}

public class OrganizationEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 2000 characters.")]
    public string Description { get; set; } = "";
}
