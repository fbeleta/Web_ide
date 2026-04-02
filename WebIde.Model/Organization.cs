namespace WebIde.Model;

public class Organization
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<User> Members { get; set; }
    public List<ProblemSet> ProblemSets { get; set; }

    public Organization()
    {
        Members = new List<User>();
        ProblemSets = new List<ProblemSet>();
    }
}
