using WebIde.Model.Enums;

namespace WebIde.Model;

public class Problem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public int MemoryLimitKb { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string AuthorUsername { get; set; }
    public List<TestCase> TestCases { get; set; }
    public List<Tag> Tags { get; set; }
    public List<Submission> Submissions { get; set; }

    public Problem()
    {
        TestCases = new List<TestCase>();
        Tags = new List<Tag>();
        Submissions = new List<Submission>();
    }
}
