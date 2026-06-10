using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Web.DTOs;

namespace WebIde.Web.Controllers.Api;

[Route("api/problem")]
public class ProblemApiController : BaseApiController
{
    private readonly WebIdeDbContext _db;

    public ProblemApiController(WebIdeDbContext db) => _db = db;

    [HttpGet]
    public ActionResult<IEnumerable<ProblemDto>> GetAll()
    {
        var problems = _db.Problems.Include(p => p.Tags).OrderBy(p => p.Id).ToList();
        return Ok(problems.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public ActionResult<ProblemDto> GetById(int id)
    {
        var problem = _db.Problems.Include(p => p.Tags).FirstOrDefault(p => p.Id == id);
        return problem is null ? NotFound() : Ok(ToDto(problem));
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<ProblemDto>> Search([FromQuery] string? q)
    {
        var query = _db.Problems.Include(p => p.Tags).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Title.Contains(q) || p.Description.Contains(q));
        return Ok(query.OrderBy(p => p.Id).ToList().Select(ToDto));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<ProblemDto> Create([FromBody] CreateProblemDto dto)
    {
        if (!Enum.TryParse<DifficultyLevel>(dto.Difficulty, true, out var difficulty))
            difficulty = DifficultyLevel.Easy;

        var tags = _db.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToList();

        var problem = new Problem
        {
            Title          = dto.Title,
            Description    = dto.Description,
            Difficulty     = difficulty,
            TimeLimitMs    = dto.TimeLimitMs,
            MemoryLimitKb  = dto.MemoryLimitKb,
            FloatTolerance = dto.FloatTolerance,
            AuthorUsername = dto.AuthorUsername,
            CreatedAt      = DateTime.UtcNow,
            Tags           = tags,
        };

        _db.Problems.Add(problem);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = problem.Id }, ToDto(problem));
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<ProblemDto> Update(int id, [FromBody] UpdateProblemDto dto)
    {
        var problem = _db.Problems.Include(p => p.Tags).FirstOrDefault(p => p.Id == id);
        if (problem is null) return NotFound();

        if (dto.Title is not null)       problem.Title       = dto.Title;
        if (dto.Description is not null) problem.Description = dto.Description;
        if (dto.FloatTolerance.HasValue) problem.FloatTolerance = dto.FloatTolerance;
        if (dto.TimeLimitMs.HasValue)    problem.TimeLimitMs = dto.TimeLimitMs.Value;
        if (dto.MemoryLimitKb.HasValue)  problem.MemoryLimitKb = dto.MemoryLimitKb.Value;
        if (dto.Difficulty is not null && Enum.TryParse<DifficultyLevel>(dto.Difficulty, true, out var diff))
            problem.Difficulty = diff;
        if (dto.TagIds is not null)
        {
            problem.Tags.Clear();
            problem.Tags = _db.Tags.Where(t => dto.TagIds.Contains(t.Id)).ToList();
        }

        _db.SaveChanges();
        return Ok(ToDto(problem));
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var problem = _db.Problems.FirstOrDefault(p => p.Id == id);
        if (problem is null) return NotFound();
        _db.Problems.Remove(problem);
        _db.SaveChanges();
        return NoContent();
    }

    private static ProblemDto ToDto(Problem p) => new()
    {
        Id             = p.Id,
        Title          = p.Title,
        Description    = p.Description,
        Difficulty     = p.Difficulty.ToString(),
        TimeLimitMs    = p.TimeLimitMs,
        MemoryLimitKb  = p.MemoryLimitKb,
        FloatTolerance = p.FloatTolerance,
        CreatedAt      = p.CreatedAt,
        AuthorUsername = p.AuthorUsername,
        Tags           = p.Tags.Select(t => t.Name).ToList(),
    };
}
