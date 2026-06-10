using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;
using WebIde.Web.Models;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("submissions")]
public class SubmissionController : Controller
{
    private readonly SubmissionRepository _repo;
    private readonly ProblemRepository _problems;
    private readonly IConnectionMultiplexer _redis;

    public SubmissionController(
        SubmissionRepository repo,
        ProblemRepository problems,
        IConnectionMultiplexer redis)
    {
        _repo = repo;
        _problems = problems;
        _redis = redis;
    }

    [Route("")]
    public IActionResult Index(string? sort)
    {
        var submissions = _repo.GetAll();

        submissions = sort switch
        {
            "date-asc"   => submissions.OrderBy(s => s.SubmittedAt).ToList(),
            "score-desc" => submissions.OrderByDescending(s => s.Score).ToList(),
            "score-asc"  => submissions.OrderBy(s => s.Score).ToList(),
            "status"     => submissions.OrderBy(s => s.Status.ToString()).ToList(),
            _            => submissions.OrderByDescending(s => s.SubmittedAt).ToList(),
        };

        ViewData["Title"] = "SUBMISSIONS";
        ViewData["sort"] = sort ?? "date-desc";
        return View(submissions);
    }

    [Route("{id:int}")]
    public IActionResult Details(int id)
    {
        var submission = _repo.GetById(id);
        if (submission is null) return NotFound();
        ViewData["Title"] = $"SUBMISSION #{id}";
        return View(submission);
    }

    [HttpPost("")]
    [Authorize]
    [EnableRateLimiting("submission")]
    public async Task<IActionResult> Create(
        [FromBody] SubmitDto dto,
        [FromServices] Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(HttpContext);

        var userIdClaim = User.FindFirstValue("webide:userId");
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.SourceCode))
            return BadRequest(new { error = "Source code cannot be empty." });

        var allowed = new[] { "cpp", "c", "python", "javascript" };
        if (!allowed.Contains(dto.Language))
            return BadRequest(new { error = $"Unsupported language: {dto.Language}" });

        var problem = _problems.GetById(dto.ProblemId);
        if (problem is null)
            return BadRequest(new { error = "Problem not found." });

        var submission = await _repo.CreateAsync(userId, dto.ProblemId, dto.Language, dto.SourceCode);

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

        return Ok(new { submissionId = submission.Id });
    }
}
