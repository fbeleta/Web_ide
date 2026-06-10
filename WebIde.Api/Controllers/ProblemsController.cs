using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebIde.Api.Data;
using WebIde.Api.DTOs;
using WebIde.Model;
using WebIde.Model.Enums;

namespace WebIde.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProblemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProblemsController(AppDbContext db) => _db = db;

    // GET /api/problems?difficulty=&tag=
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProblemDto>>> GetAll(
        [FromQuery] string? difficulty,
        [FromQuery] string? tag)
    {
        var query = _db.Problems.Include(p => p.Tags).AsQueryable();

        if (difficulty is not null)
        {
            if (!Enum.TryParse<DifficultyLevel>(difficulty, ignoreCase: true, out var level))
                return ValidationProblem(new ValidationProblemDetails
                {
                    Title = "Invalid difficulty value",
                    Detail = $"'{difficulty}' is not a valid difficulty. Use Easy, Medium, or Hard."
                });

            query = query.Where(p => p.Difficulty == level);
        }

        if (tag is not null)
            query = query.Where(p => p.Tags.Any(t => t.Name == tag));

        var problems = await query.ToListAsync();
        return Ok(problems.Select(ToDto));
    }

    // GET /api/problems/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ProblemDto>> GetById(int id)
    {
        var problem = await _db.Problems
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (problem is null)
            return NotFound(new ProblemDetails
            {
                Title = "Problem not found",
                Detail = $"No problem with id {id} exists.",
                Status = 404
            });

        return Ok(ToDto(problem));
    }

    // POST /api/problems
    [HttpPost]
    public async Task<ActionResult<ProblemDto>> Create([FromBody] CreateProblemRequest req)
    {
        if (!Enum.TryParse<DifficultyLevel>(req.Difficulty, ignoreCase: true, out var level))
            return ValidationProblem(new ValidationProblemDetails
            {
                Title = "Invalid difficulty value",
                Detail = $"'{req.Difficulty}' is not a valid difficulty. Use Easy, Medium, or Hard."
            });

        var problem = new Problem
        {
            Title = req.Title,
            Description = req.Description,
            Difficulty = level,
            TimeLimitMs = req.TimeLimitMs,
            MemoryLimitKb = req.MemoryLimitKb,
            AuthorUsername = req.AuthorUsername,
            CreatedAt = DateTime.UtcNow
        };

        if (req.Tags is { Count: > 0 })
            problem.Tags = await ResolveTagsAsync(req.Tags);

        _db.Problems.Add(problem);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = problem.Id }, ToDto(problem));
    }

    // PUT /api/problems/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<ProblemDto>> Update(int id, [FromBody] UpdateProblemRequest req)
    {
        var problem = await _db.Problems
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (problem is null)
            return NotFound(new ProblemDetails
            {
                Title = "Problem not found",
                Detail = $"No problem with id {id} exists.",
                Status = 404
            });

        if (!Enum.TryParse<DifficultyLevel>(req.Difficulty, ignoreCase: true, out var level))
            return ValidationProblem(new ValidationProblemDetails
            {
                Title = "Invalid difficulty value",
                Detail = $"'{req.Difficulty}' is not a valid difficulty. Use Easy, Medium, or Hard."
            });

        problem.Title = req.Title;
        problem.Description = req.Description;
        problem.Difficulty = level;
        problem.TimeLimitMs = req.TimeLimitMs;
        problem.MemoryLimitKb = req.MemoryLimitKb;

        problem.Tags.Clear();
        if (req.Tags is { Count: > 0 })
            problem.Tags = await ResolveTagsAsync(req.Tags);

        await _db.SaveChangesAsync();
        return Ok(ToDto(problem));
    }

    // DELETE /api/problems/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var problem = await _db.Problems.FindAsync(id);
        if (problem is null)
            return NotFound(new ProblemDetails
            {
                Title = "Problem not found",
                Detail = $"No problem with id {id} exists.",
                Status = 404
            });

        _db.Problems.Remove(problem);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProblemDto ToDto(Problem p) => new(
        p.Id,
        p.Title,
        p.Description,
        p.Difficulty.ToString(),
        p.TimeLimitMs,
        p.MemoryLimitKb,
        p.CreatedAt,
        p.AuthorUsername,
        p.Tags.Select(t => t.Name).ToList()
    );

    private async Task<List<Tag>> ResolveTagsAsync(List<string> names)
    {
        var result = new List<Tag>();
        foreach (var name in names)
        {
            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == name)
                      ?? new Tag { Name = name };
            result.Add(tag);
        }
        return result;
    }
}
