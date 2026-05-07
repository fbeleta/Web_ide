using System.ComponentModel.DataAnnotations;

namespace WebIde.Model;

public class Organization
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public virtual ICollection<User> Members { get; set; } = new List<User>();
    public virtual ICollection<ProblemSet> ProblemSets { get; set; } = new List<ProblemSet>();
}
