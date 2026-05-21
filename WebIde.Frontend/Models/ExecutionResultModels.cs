using System.ComponentModel.DataAnnotations;

namespace WebIde.Frontend.Models;

public class ExecutionResultCreateModel
{
    public int SubmissionId { get; set; }

    [Required(ErrorMessage = "Stdout is required.")]
    [StringLength(50000)]
    public string Stdout { get; set; } = "";

    [StringLength(50000)]
    public string Stderr { get; set; } = "";

    public int ExitCode { get; set; } = 0;

    public bool TimedOut { get; set; } = false;

    public bool MemoryExceeded { get; set; } = false;
}
