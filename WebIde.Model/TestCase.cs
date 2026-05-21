using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebIde.Model;

public class TestCase
{
    [Key]
    public int Id { get; set; }
    public required string InputArgs { get; set; }
    public required string ExpectedOutput { get; set; }
    public bool IsSample { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int ProblemId { get; set; }
    [ForeignKey("ProblemId")]
    public virtual Problem Problem { get; set; } = null!;
}
