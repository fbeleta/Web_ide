namespace WebIde.Model;

public class TestCase
{
    public int Id { get; set; }
    public required string InputArgs { get; set; }
    public required string ExpectedOutput { get; set; }
    public bool IsSample { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public required Problem Problem { get; set; }
}
