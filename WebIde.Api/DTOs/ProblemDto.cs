namespace WebIde.Api.DTOs;

public record ProblemDto(
    int Id,
    string Title,
    string Description,
    string Difficulty,
    int TimeLimitMs,
    int MemoryLimitKb,
    DateTime CreatedAt,
    string AuthorUsername,
    IReadOnlyList<string> Tags
);
