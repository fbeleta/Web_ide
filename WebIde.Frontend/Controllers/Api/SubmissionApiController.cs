using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;
using WebIde.Web.DTOs;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers.Api;

[Route("api/submission")]
public class SubmissionApiController : BaseApiController
{
    private readonly SubmissionRepository _repo;
    private readonly ProblemRepository _problems;
    private readonly IConnectionMultiplexer _redis;

    public SubmissionApiController(
        SubmissionRepository repo,
        ProblemRepository problems,
        IConnectionMultiplexer redis)
    {
        _repo     = repo;
        _problems = problems;
        _redis    = redis;
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public ActionResult<IEnumerable<SubmissionDto>> GetAll() =>
        Ok(_repo.GetAll().Select(ToDto));

    [HttpGet("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity)]
    public ActionResult<SubmissionDto> GetById(int id)
    {
        var s = _repo.GetById(id);
        if (s is null) return NotFound();

        // Non-admins may only see their own submissions
        if (!User.IsInRole("Admin"))
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (s.UserId.ToString() != callerId) return Forbid();
        }

        return Ok(ToDto(s));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity)]
    [EnableRateLimiting("submission")]
    public async Task<ActionResult<SubmissionDto>> Create([FromBody] CreateSubmissionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // Map Identity user ID to domain user ID via email
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue(ClaimTypes.Name)
                 ?? "";

        // For API submissions, we use a synthetic userId of 0 if no matching domain user exists.
        // The worker will still process the submission.
        if (!int.TryParse(userId, out var domainUserId)) domainUserId = 0;

        var allowed = new[] { "cpp", "c", "python", "javascript" };
        if (!allowed.Contains(dto.Language))
            return BadRequest(new { error = $"Unsupported language: {dto.Language}" });

        var problem = _problems.GetById(dto.ProblemId);
        if (problem is null) return BadRequest(new { error = "Problem not found." });

        var submission = await _repo.CreateAsync(domainUserId, dto.ProblemId, dto.Language, dto.SourceCode);

        var job = new
        {
            SubmissionId  = submission.Id,
            ProblemId     = dto.ProblemId,
            Language      = dto.Language,
            SourceCode    = dto.SourceCode,
            TimeLimitMs   = problem.TimeLimitMs,
            MemoryLimitKb = problem.MemoryLimitKb,
        };
        await _redis.GetDatabase().ListRightPushAsync(
            "submissions:queue",
            JsonSerializer.Serialize(job));

        return CreatedAtAction(nameof(GetById), new { id = submission.Id }, ToDto(submission));
    }

    private static SubmissionDto ToDto(WebIde.Model.Submission s) => new()
    {
        Id          = s.Id,
        Language    = s.Language,
        Status      = s.Status.ToString(),
        SourceCode  = s.SourceCode,
        SubmittedAt = s.SubmittedAt,
        Score       = s.Score,
        WallTimeMs  = s.WallTimeMs,
        PeakMemoryKb = s.PeakMemoryKb,
        UserId      = s.UserId,
        ProblemId   = s.ProblemId,
    };
}
