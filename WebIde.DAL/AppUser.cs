using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebIde.DAL;

public class AppUser : IdentityUser
{
    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
    public string OIB { get; set; } = "";

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadržavati samo brojeve.")]
    public string JMBG { get; set; } = "";
}
