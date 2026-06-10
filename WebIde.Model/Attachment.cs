using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebIde.Model;

public class Attachment
{
    [Key]
    public int Id { get; set; }

    public required string FileName { get; set; }

    public required string StoredFileName { get; set; }

    public required string ContentType { get; set; }

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ProblemId { get; set; }

    [ForeignKey("ProblemId")]
    public virtual Problem Problem { get; set; } = null!;
}
