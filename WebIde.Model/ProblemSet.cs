namespace WebIde.Model;

public class ProblemSet
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public int OrderIndex { get; set; }
    public required Organization Organization { get; set; }
    public List<Problem> Problems { get; set; }

    public ProblemSet()
    {
        Problems = new List<Problem>();
    }
}
