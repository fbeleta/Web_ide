using System.ComponentModel.DataAnnotations;

namespace WebIde.Web.DTOs;

// Returned to any authenticated or public caller — no expected output.
public class TestCasePublicDto
{
    public int Id { get; set; }
    public string InputArgs { get; set; } = "";
    public bool IsSample { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public int ProblemId { get; set; }
}

// Admin-only — includes expected output for judging configuration.
public class TestCaseDto
{
    public int Id { get; set; }
    public string InputArgs { get; set; } = "";
    public string ExpectedOutput { get; set; } = "";
    public bool IsSample { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; }
    public int ProblemId { get; set; }
}

public class CreateTestCaseDto
{
    [Required]
    public int ProblemId { get; set; }

    [Required]
    public string InputArgs { get; set; } = "";

    [Required]
    public string ExpectedOutput { get; set; } = "";

    public bool IsSample { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; } = 1;
}

public class UpdateTestCaseDto
{
    public string? InputArgs { get; set; }
    public string? ExpectedOutput { get; set; }
    public bool? IsSample { get; set; }
    public int? OrderIndex { get; set; }
    public int? Points { get; set; }
}
