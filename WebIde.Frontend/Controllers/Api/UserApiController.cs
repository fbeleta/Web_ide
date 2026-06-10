using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Web.DTOs;

namespace WebIde.Web.Controllers.Api;

[Route("api/user")]
public class UserApiController : BaseApiController
{
    private readonly WebIdeDbContext _db;
    public UserApiController(WebIdeDbContext db) => _db = db;

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetAll() =>
        Ok(_db.DomainUsers.OrderBy(u => u.Id).ToList().Select(ToDto));

    [HttpGet("{id:int}")]
    public ActionResult<UserDto> GetById(int id)
    {
        var user = _db.DomainUsers.FirstOrDefault(u => u.Id == id);
        return user is null ? NotFound() : Ok(ToDto(user));
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<UserDto>> Search([FromQuery] string? q)
    {
        var query = _db.DomainUsers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Username.Contains(q) || u.DisplayName.Contains(q));
        return Ok(query.OrderBy(u => u.Id).ToList().Select(ToDto));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public ActionResult<UserDto> Create([FromBody] CreateUserDto dto)
    {
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role)) role = UserRole.Student;
        var user = new User
        {
            Username     = dto.Username,
            Email        = dto.Email,
            DisplayName  = dto.DisplayName,
            Role         = role,
            RegisteredAt = DateTime.UtcNow,
        };
        _db.DomainUsers.Add(user);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToDto(user));
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public ActionResult<UserDto> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = _db.DomainUsers.FirstOrDefault(u => u.Id == id);
        if (user is null) return NotFound();
        if (dto.DisplayName is not null) user.DisplayName = dto.DisplayName;
        if (dto.Email is not null)       user.Email       = dto.Email;
        if (dto.Role is not null && Enum.TryParse<UserRole>(dto.Role, true, out var role))
            user.Role = role;
        _db.SaveChanges();
        return Ok(ToDto(user));
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var user = _db.DomainUsers.FirstOrDefault(u => u.Id == id);
        if (user is null) return NotFound();
        _db.DomainUsers.Remove(user);
        _db.SaveChanges();
        return NoContent();
    }

    private static UserDto ToDto(User u) => new()
    {
        Id          = u.Id,
        Username    = u.Username,
        DisplayName = u.DisplayName,
        Email       = u.Email,
        Role        = u.Role.ToString(),
        RegisteredAt = u.RegisteredAt,
    };
}
