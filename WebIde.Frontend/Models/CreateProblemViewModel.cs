using System.ComponentModel.DataAnnotations;
using WebIde.Model;

namespace WebIde.Web.Models;

public class CreateProblemViewModel
{
    public int? Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    public string Difficulty { get; set; } = "Easy";

    [Range(100, 30000)]
    public int TimeLimitMs { get; set; } = 2000;

    [Range(1024, 1048576)]
    public int MemoryLimitKb { get; set; } = 262144;

    public double? FloatTolerance { get; set; }

    public string? AuthorUsername { get; set; }

    public List<int>? TagIds { get; set; }

    // Only populated in Edit view (not used for binding)
    public List<Attachment> Attachments { get; set; } = [];
}
