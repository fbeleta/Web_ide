using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Web.Models;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("submissions")]
public class SubmissionController : Controller
{
    private readonly SubmissionRepository _repo;
    private readonly ProblemRepository _problems;
    private readonly UserRepository _users;
    private readonly IConnectionMultiplexer _redis;

    public SubmissionController(
        SubmissionRepository repo,
        ProblemRepository problems,
        UserRepository users,
        IConnectionMultiplexer redis)
    {
        _repo     = repo;
        _problems = problems;
        _users    = users;
        _redis    = redis;
    }

    [Route("")]
    [Authorize]
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
    [Authorize]
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
    public async Task<IActionResult> Submit(
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

    [Route("create")]
    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE SUBMISSION";
        return View(new SubmissionCreateModel());
    }

    [Route("create")]
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult CreateAdmin(SubmissionCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "CREATE SUBMISSION";
            return View("Create", model);
        }

        var user = _users.GetById(model.UserId);
        if (user is null) { ModelState.AddModelError(nameof(model.UserId), "User not found."); return View("Create", model); }

        var problem = _problems.GetById(model.ProblemId);
        if (problem is null) { ModelState.AddModelError(nameof(model.ProblemId), "Problem not found."); return View("Create", model); }

        _repo.Add(new Submission
        {
            SourceCode   = model.SourceCode,
            Language     = model.Language,
            Status       = model.Status,
            SubmittedAt  = model.SubmittedAt,
            Score        = model.Score,
            WallTimeMs   = model.WallTimeMs,
            PeakMemoryKb = model.PeakMemoryKb,
            UserId       = model.UserId,
            ProblemId    = model.ProblemId,
        });
        TempData["Flash"] = "Submission created.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/edit")]
    [HttpGet, ActionName("Edit")]
    [Authorize]
    public IActionResult EditGet(int id)
    {
        var s = _repo.GetById(id);
        if (s is null) return NotFound();
        ViewData["Title"] = "EDIT SUBMISSION";
        return View(new SubmissionEditModel
        {
            Id              = s.Id,
            SourceCode      = s.SourceCode,
            Language        = s.Language,
            Status          = s.Status,
            SubmittedAt     = s.SubmittedAt,
            Score           = s.Score,
            WallTimeMs      = s.WallTimeMs,
            PeakMemoryKb    = s.PeakMemoryKb,
            UserId          = s.UserId,
            UserDisplayName = s.User?.DisplayName ?? "",
            ProblemId       = s.ProblemId,
            ProblemTitle    = s.Problem?.Title ?? "",
        });
    }

    [Route("{id:int}/edit")]
    [HttpPost, ActionName("Edit")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(int id, SubmissionEditModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "EDIT SUBMISSION";
            return View(model);
        }
        var s = _repo.GetById(id);
        if (s is null) return NotFound();

        s.SourceCode   = model.SourceCode;
        s.Language     = model.Language;
        s.Status       = model.Status;
        s.SubmittedAt  = model.SubmittedAt;
        s.Score        = model.Score;
        s.WallTimeMs   = model.WallTimeMs;
        s.PeakMemoryKb = model.PeakMemoryKb;
        s.UserId       = model.UserId;
        s.ProblemId    = model.ProblemId;
        _repo.Update();
        TempData["Flash"] = $"Submission #{id} updated.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/delete")]
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _repo.SoftDelete(id);
        TempData["Flash"] = "Submission deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Route("search")]
    [HttpGet]
    public IActionResult Search(string q)
    {
        var results = _repo.Search(q ?? "");
        return Json(results.Select(s => new
        {
            id    = s.Id,
            label = $"#{s.Id} — {s.Problem?.Title ?? "?"} ({s.Language})",
        }));
    }
}
