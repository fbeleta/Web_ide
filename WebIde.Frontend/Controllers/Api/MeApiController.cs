using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace WebIde.Web.Controllers.Api;

// Per-user scratch state shared across headless clients. The VS Code extension
// sets the "current problem" when you open one; the MCP server reads it so Claude
// can act on "the current task" without being told an id.
[Route("api/me")]
public class MeApiController : BaseApiController
{
    private readonly IConnectionMultiplexer _redis;
    public MeApiController(IConnectionMultiplexer redis) => _redis = redis;

    public record CurrentProblemDto(int? ProblemId);

    private string? CurrentProblemKey()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(id) ? null : $"user:{id}:currentProblem";
    }

    [HttpGet("current-problem")]
    public async Task<ActionResult<CurrentProblemDto>> GetCurrentProblem()
    {
        var key = CurrentProblemKey();
        if (key is null) return Unauthorized();

        var value = await _redis.GetDatabase().StringGetAsync(key);
        return Ok(new CurrentProblemDto(value.IsNullOrEmpty ? null : (int)value));
    }

    [HttpPut("current-problem")]
    public async Task<ActionResult<CurrentProblemDto>> SetCurrentProblem([FromBody] CurrentProblemDto dto)
    {
        var key = CurrentProblemKey();
        if (key is null) return Unauthorized();

        var db = _redis.GetDatabase();
        if (dto.ProblemId is null)
            await db.KeyDeleteAsync(key);
        else
            await db.StringSetAsync(key, dto.ProblemId.Value);

        return Ok(dto);
    }
}
