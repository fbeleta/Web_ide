namespace WebIde.Model;

public class ExecutionResult
{
    public int Id { get; set; }
    public required string Stdout { get; set; }
    public required string Stderr { get; set; }
    public int ExitCode { get; set; }
    public bool TimedOut { get; set; }
    public bool MemoryExceeded { get; set; }
}
