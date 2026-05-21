using System.ComponentModel.DataAnnotations;

namespace WebIde.Model;

public class Tag
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime? DeletedAt { get; set; }
    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
