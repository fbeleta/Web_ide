using System.ComponentModel.DataAnnotations;

namespace WebIde.Frontend.Models;

public class ProblemSetCreateModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 2000 characters.")]
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPublic { get; set; } = true;

    [Range(0, 9999, ErrorMessage = "Order index must be between 0 and 9999.")]
    public int OrderIndex { get; set; } = 0;

    [Range(1, int.MaxValue, ErrorMessage = "Please select an organization.")]
    public int OrganizationId { get; set; }

    // Display text for the autocomplete (not bound to model)
    public string OrganizationName { get; set; } = "";
}

public class ProblemSetEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 2000 characters.")]
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public bool IsPublic { get; set; }

    [Range(0, 9999, ErrorMessage = "Order index must be between 0 and 9999.")]
    public int OrderIndex { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select an organization.")]
    public int OrganizationId { get; set; }

    public string OrganizationName { get; set; } = "";
}
