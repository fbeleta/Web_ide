using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Web.DTOs;

namespace WebIde.Web.Controllers.Api;

[Route("api/problemset")]
public class ProblemSetApiController : BaseApiController
{
    private readonly WebIdeDbContext _db;
    public ProblemSetApiController(WebIdeDbContext db) => _db = db;

    [HttpGet]
    public ActionResult<IEnumerable<ProblemSetDto>> GetAll()
    {
        var sets = _db.ProblemSets.Include(ps => ps.Problems).OrderBy(ps => ps.Id).ToList();
        return Ok(sets.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public ActionResult<ProblemSetDto> GetById(int id)
    {
        var ps = _db.ProblemSets.Include(ps => ps.Problems).FirstOrDefault(ps => ps.Id == id);
        return ps is null ? NotFound() : Ok(ToDto(ps));
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<ProblemSetDto>> Search([FromQuery] string? q)
    {
        var query = _db.ProblemSets.Include(ps => ps.Problems).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(ps => ps.Title.Contains(q) || ps.Description.Contains(q));
        return Ok(query.OrderBy(ps => ps.Id).ToList().Select(ToDto));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<ProblemSetDto> Create([FromBody] CreateProblemSetDto dto)
    {
        var problems = _db.Problems.Where(p => dto.ProblemIds.Contains(p.Id)).ToList();
        var ps = new ProblemSet
        {
            Title          = dto.Title,
            Description    = dto.Description,
            IsPublic       = dto.IsPublic,
            OrganizationId = dto.OrganizationId,
            CreatedAt      = DateTime.UtcNow,
            Problems       = problems,
        };
        _db.ProblemSets.Add(ps);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = ps.Id }, ToDto(ps));
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<ProblemSetDto> Update(int id, [FromBody] UpdateProblemSetDto dto)
    {
        var ps = _db.ProblemSets.Include(ps => ps.Problems).FirstOrDefault(ps => ps.Id == id);
        if (ps is null) return NotFound();
        if (dto.Title is not null)       ps.Title       = dto.Title;
        if (dto.Description is not null) ps.Description = dto.Description;
        if (dto.IsPublic.HasValue)       ps.IsPublic    = dto.IsPublic.Value;
        if (dto.ProblemIds is not null)
        {
            ps.Problems.Clear();
            ps.Problems = _db.Problems.Where(p => dto.ProblemIds.Contains(p.Id)).ToList();
        }
        _db.SaveChanges();
        return Ok(ToDto(ps));
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var ps = _db.ProblemSets.FirstOrDefault(ps => ps.Id == id);
        if (ps is null) return NotFound();
        _db.ProblemSets.Remove(ps);
        _db.SaveChanges();
        return NoContent();
    }

    private static ProblemSetDto ToDto(ProblemSet ps) => new()
    {
        Id             = ps.Id,
        Title          = ps.Title,
        Description    = ps.Description,
        CreatedAt      = ps.CreatedAt,
        IsPublic       = ps.IsPublic,
        OrganizationId = ps.OrganizationId,
        ProblemIds     = ps.Problems.Select(p => p.Id).ToList(),
    };
}
