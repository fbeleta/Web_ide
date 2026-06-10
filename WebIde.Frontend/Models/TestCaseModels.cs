using System.ComponentModel.DataAnnotations;

namespace WebIde.Frontend.Models;

public class TestCaseCreateModel
{
    public int ProblemId { get; set; }

    [Required(ErrorMessage = "Input args are required.")]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Input args must not be empty.")]
    public string InputArgs { get; set; } = "";

    [Required(ErrorMessage = "Expected output is required.")]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Expected output must not be empty.")]
    public string ExpectedOutput { get; set; } = "";

    public bool IsSample { get; set; } = false;

    [Range(0, 9999, ErrorMessage = "Order index must be between 0 and 9999.")]
    public int OrderIndex { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "Points must be between 0 and 100.")]
    public int Points { get; set; } = 1;
}

public class TestCaseEditModel
{
    public int Id { get; set; }

    public int ProblemId { get; set; }

    [Required(ErrorMessage = "Input args are required.")]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Input args must not be empty.")]
    public string InputArgs { get; set; } = "";

    [Required(ErrorMessage = "Expected output is required.")]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Expected output must not be empty.")]
    public string ExpectedOutput { get; set; } = "";

    public bool IsSample { get; set; }

    [Range(0, 9999, ErrorMessage = "Order index must be between 0 and 9999.")]
    public int OrderIndex { get; set; }

    [Range(0, 100, ErrorMessage = "Points must be between 0 and 100.")]
    public int Points { get; set; }
}
