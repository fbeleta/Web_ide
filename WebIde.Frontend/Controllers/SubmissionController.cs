using Microsoft.AspNetCore.Mvc;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("submissions")]
public class SubmissionController : Controller
{
    private readonly SubmissionRepository _repo;
    private readonly UserRepository _users;
    private readonly ProblemRepository _problems;

    public SubmissionController(SubmissionRepository repo, UserRepository users, ProblemRepository problems)
    {
        _repo = repo;
        _users = users;
        _problems = problems;
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

    [Route("create")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE SUBMISSION";
        return View(new SubmissionCreateModel());
    }

    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SubmissionCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "CREATE SUBMISSION";
            return View(model);
        }

        var user = _users.GetById(model.UserId);
        if (user is null) { ModelState.AddModelError(nameof(model.UserId), "User not found."); return View(model); }

        var problem = _problems.GetById(model.ProblemId);
        if (problem is null) { ModelState.AddModelError(nameof(model.ProblemId), "Problem not found."); return View(model); }

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
    public IActionResult EditGet(int id)
    {
        var s = _repo.GetById(id);
        if (s is null) return NotFound();
        ViewData["Title"] = "EDIT SUBMISSION";
        return View(new SubmissionEditModel
        {
            Id           = s.Id,
            SourceCode   = s.SourceCode,
            Language     = s.Language,
            Status       = s.Status,
            SubmittedAt  = s.SubmittedAt,
            Score        = s.Score,
            WallTimeMs   = s.WallTimeMs,
            PeakMemoryKb = s.PeakMemoryKb,
            UserId       = s.UserId,
            UserDisplayName = s.User?.DisplayName ?? "",
            ProblemId    = s.ProblemId,
            ProblemTitle = s.Problem?.Title ?? "",
        });
    }

    [Route("{id:int}/edit")]
    [HttpPost, ActionName("Edit")]
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
