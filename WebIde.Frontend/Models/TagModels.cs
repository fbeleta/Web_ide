using System.ComponentModel.DataAnnotations;

namespace WebIde.Frontend.Models;

public class TagCreateModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters.")]
    public string Name { get; set; } = "";
}

public class TagEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters.")]
    public string Name { get; set; } = "";
}
