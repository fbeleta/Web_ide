namespace WebIde.Model;

public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<Problem> Problems { get; set; }

    public Tag()
    {
        Problems = new List<Problem>();
    }
}
