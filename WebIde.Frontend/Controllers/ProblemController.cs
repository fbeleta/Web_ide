using Microsoft.AspNetCore.Mvc;
using WebIde.DAL;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("problems")]
public class ProblemController : Controller
{
    private readonly ProblemRepository _repo;
    private readonly SubmissionRepository _submissions;
    private readonly TagRepository _tags;
    private readonly WebIdeDbContext _db;

    public ProblemController(ProblemRepository repo, SubmissionRepository submissions, TagRepository tags, WebIdeDbContext db)
    {
        _repo = repo;
        _submissions = submissions;
        _tags = tags;
        _db = db;
    }

    [Route("")]
    public IActionResult Index(string? sort)
    {
        var problems = _repo.GetAll();

        problems = sort switch
        {
            "difficulty-asc"  => problems.OrderBy(p => p.Difficulty).ToList(),
            "difficulty-desc" => problems.OrderByDescending(p => p.Difficulty).ToList(),
            "title"           => problems.OrderBy(p => p.Title).ToList(),
            "acceptance-asc"  => problems.OrderBy(p =>
                p.Submissions.Count == 0 ? 0 : (double)p.Submissions.Count(s => s.Status == SubmissionStatus.Accepted) / p.Submissions.Count).ToList(),
            "acceptance-desc" => problems.OrderByDescending(p =>
                p.Submissions.Count == 0 ? 0 : (double)p.Submissions.Count(s => s.Status == SubmissionStatus.Accepted) / p.Submissions.Count).ToList(),
            _                 => problems.OrderBy(p => p.Id).ToList(),
        };

        ViewData["Title"] = "PROBLEMS";
        ViewData["sort"] = sort ?? "";
        return View(problems);
    }

    [Route("{id:int}")]
    public IActionResult Details(int id)
    {
        var problem = _repo.GetById(id);
        if (problem is null) return NotFound();
        ViewData["Title"] = problem.Title.ToUpper();
        return View(problem);
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE PROBLEM";
        return View(new ProblemCreateModel());
    }

    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProblemCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "CREATE PROBLEM";
            return View(model);
        }
        var problem = new Problem
        {
            Title         = model.Title,
            Description   = model.Description,
            Difficulty    = model.Difficulty,
            TimeLimitMs   = model.TimeLimitMs,
            MemoryLimitKb = model.MemoryLimitKb,
            AuthorUsername = model.AuthorUsername,
            CreatedAt     = model.CreatedAt,
        };

        if (model.TagIds.Any())
        {
            var tags = _db.Tags.Where(t => model.TagIds.Contains(t.Id)).ToList();
            foreach (var tag in tags) problem.Tags.Add(tag);
        }

        _repo.Add(problem);
        TempData["Flash"] = $"Problem \"{model.Title}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/edit")]
    [HttpGet, ActionName("Edit")]
    public IActionResult EditGet(int id)
    {
        var problem = _repo.GetById(id);
        if (problem is null) return NotFound();
        ViewData["Title"] = "EDIT PROBLEM";
        return View(new ProblemEditModel
        {
            Id            = problem.Id,
            Title         = problem.Title,
            Description   = problem.Description,
            Difficulty    = problem.Difficulty,
            TimeLimitMs   = problem.TimeLimitMs,
            MemoryLimitKb = problem.MemoryLimitKb,
            AuthorUsername = problem.AuthorUsername,
            CreatedAt     = problem.CreatedAt,
            TagIds        = problem.Tags.Select(t => t.Id).ToList(),
        });
    }

    [Route("{id:int}/edit")]
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(int id, ProblemEditModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "EDIT PROBLEM";
            return View(model);
        }
        var problem = _repo.GetById(id);
        if (problem is null) return NotFound();

        problem.Title         = model.Title;
        problem.Description   = model.Description;
        problem.Difficulty    = model.Difficulty;
        problem.TimeLimitMs   = model.TimeLimitMs;
        problem.MemoryLimitKb = model.MemoryLimitKb;
        problem.AuthorUsername = model.AuthorUsername;
        problem.CreatedAt     = model.CreatedAt;

        // Sync tags
        problem.Tags.Clear();
        if (model.TagIds.Any())
        {
            var tags = _db.Tags.Where(t => model.TagIds.Contains(t.Id)).ToList();
            foreach (var tag in tags) problem.Tags.Add(tag);
        }

        _repo.Update();
        TempData["Flash"] = $"Problem \"{model.Title}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _repo.SoftDelete(id);
        TempData["Flash"] = "Problem deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Route("search")]
    [HttpGet]
    public IActionResult Search(string q)
    {
        var results = _repo.Search(q ?? "");
        return Json(results.Select(p => new { id = p.Id, label = p.Title }));
    }
}
