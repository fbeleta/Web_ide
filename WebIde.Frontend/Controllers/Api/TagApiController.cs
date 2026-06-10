using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Web.DTOs;

namespace WebIde.Web.Controllers.Api;

[Route("api/tag")]
public class TagApiController : BaseApiController
{
    private readonly WebIdeDbContext _db;
    public TagApiController(WebIdeDbContext db) => _db = db;

    [HttpGet]
    public ActionResult<IEnumerable<TagDto>> GetAll() =>
        Ok(_db.Tags.OrderBy(t => t.Name).ToList().Select(ToDto));

    [HttpGet("{id:int}")]
    public ActionResult<TagDto> GetById(int id)
    {
        var tag = _db.Tags.FirstOrDefault(t => t.Id == id);
        return tag is null ? NotFound() : Ok(ToDto(tag));
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<TagDto>> Search([FromQuery] string? q)
    {
        var tags = _db.Tags.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            tags = tags.Where(t => t.Name.Contains(q));
        return Ok(tags.OrderBy(t => t.Name).ToList().Select(ToDto));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<TagDto> Create([FromBody] CreateTagDto dto)
    {
        var tag = new Tag { Name = dto.Name };
        _db.Tags.Add(tag);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, ToDto(tag));
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<TagDto> Update(int id, [FromBody] UpdateTagDto dto)
    {
        var tag = _db.Tags.FirstOrDefault(t => t.Id == id);
        if (tag is null) return NotFound();
        if (dto.Name is not null) tag.Name = dto.Name;
        _db.SaveChanges();
        return Ok(ToDto(tag));
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var tag = _db.Tags.FirstOrDefault(t => t.Id == id);
        if (tag is null) return NotFound();
        _db.Tags.Remove(tag);
        _db.SaveChanges();
        return NoContent();
    }

    private static TagDto ToDto(Tag t) => new() { Id = t.Id, Name = t.Name };
}
