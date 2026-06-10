using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Web.DTOs;

namespace WebIde.Web.Controllers.Api;

[Route("api/organization")]
public class OrganizationApiController : BaseApiController
{
    private readonly WebIdeDbContext _db;
    public OrganizationApiController(WebIdeDbContext db) => _db = db;

    [HttpGet]
    public ActionResult<IEnumerable<OrganizationDto>> GetAll()
    {
        var orgs = _db.Organizations.Include(o => o.Members).OrderBy(o => o.Id).ToList();
        return Ok(orgs.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public ActionResult<OrganizationDto> GetById(int id)
    {
        var org = _db.Organizations.Include(o => o.Members).FirstOrDefault(o => o.Id == id);
        return org is null ? NotFound() : Ok(ToDto(org));
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<OrganizationDto>> Search([FromQuery] string? q)
    {
        var query = _db.Organizations.Include(o => o.Members).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(o => o.Name.Contains(q) || o.Description.Contains(q));
        return Ok(query.OrderBy(o => o.Id).ToList().Select(ToDto));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<OrganizationDto> Create([FromBody] CreateOrganizationDto dto)
    {
        var org = new Organization { Name = dto.Name, Description = dto.Description };
        _db.Organizations.Add(org);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = org.Id }, ToDto(org));
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin,Manager")]
    public ActionResult<OrganizationDto> Update(int id, [FromBody] UpdateOrganizationDto dto)
    {
        var org = _db.Organizations.Include(o => o.Members).FirstOrDefault(o => o.Id == id);
        if (org is null) return NotFound();
        if (dto.Name is not null)        org.Name        = dto.Name;
        if (dto.Description is not null) org.Description = dto.Description;
        _db.SaveChanges();
        return Ok(ToDto(org));
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var org = _db.Organizations.FirstOrDefault(o => o.Id == id);
        if (org is null) return NotFound();
        _db.Organizations.Remove(org);
        _db.SaveChanges();
        return NoContent();
    }

    private static OrganizationDto ToDto(Organization o) => new()
    {
        Id          = o.Id,
        Name        = o.Name,
        Description = o.Description,
        MemberIds   = o.Members.Select(m => m.Id).ToList(),
    };
}
