using System.ComponentModel.DataAnnotations;
using WebIde.Model.Enums;

namespace WebIde.Model;

public class Problem
{
    [Key]
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public int MemoryLimitKb { get; set; }
    public double? FloatTolerance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public required string AuthorUsername { get; set; }
    public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
