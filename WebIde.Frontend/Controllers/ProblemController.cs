using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Web.DTOs;
using WebIde.Web.Models;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("problems")]
public class ProblemController : Controller
{
    private readonly ProblemRepository _repo;
    private readonly SubmissionRepository _submissions;
    private readonly WebIdeDbContext _db;

    public ProblemController(ProblemRepository repo, SubmissionRepository submissions, WebIdeDbContext db)
    {
        _repo        = repo;
        _submissions = submissions;
        _db          = db;
    }

    // ── Public read actions ───────────────────────────────────────────────────

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

    // ── Search (AJAX autocomplete) ─────────────────────────────────────────────

    [Route("search")]
    [HttpGet]
    public IActionResult Search(string q)
    {
        var results = _repo.Search(q ?? "");
        return Json(results.Select(p => new { id = p.Id, label = p.Title }));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpGet("create")]
    [Authorize(Roles = "Admin,Instructor")]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE PROBLEM";
        ViewData["Tags"] = _db.Tags.OrderBy(t => t.Name).ToList();
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public IActionResult Create(CreateProblemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Tags"] = _db.Tags.OrderBy(t => t.Name).ToList();
            return View(model);
        }

        if (!Enum.TryParse<DifficultyLevel>(model.Difficulty, true, out var difficulty))
            difficulty = DifficultyLevel.Easy;

        var tagIds = model.TagIds ?? new List<int>();
        var tags = _db.Tags.Where(t => tagIds.Contains(t.Id)).ToList();

        var problem = new Problem
        {
            Title          = model.Title,
            Description    = model.Description,
            Difficulty     = difficulty,
            TimeLimitMs    = model.TimeLimitMs,
            MemoryLimitKb  = model.MemoryLimitKb,
            FloatTolerance = model.FloatTolerance,
            AuthorUsername = model.AuthorUsername ?? "",
            CreatedAt      = DateTime.UtcNow,
            Tags           = tags,
        };

        _db.Problems.Add(problem);
        _db.SaveChanges();

        TempData["Flash"] = $"Problem \"{model.Title}\" created.";
        return RedirectToAction(nameof(Details), new { id = problem.Id });
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/edit")]
    [Authorize(Roles = "Admin,Instructor")]
    public IActionResult Edit(int id)
    {
        var problem = _db.Problems.Include(p => p.Tags).Include(p => p.Attachments).FirstOrDefault(p => p.Id == id);
        if (problem is null) return NotFound();

        ViewData["Title"] = $"EDIT: {problem.Title.ToUpper()}";
        ViewData["Tags"] = _db.Tags.OrderBy(t => t.Name).ToList();

        var model = new CreateProblemViewModel
        {
            Id             = problem.Id,
            Title          = problem.Title,
            Description    = problem.Description,
            Difficulty     = problem.Difficulty.ToString(),
            TimeLimitMs    = problem.TimeLimitMs,
            MemoryLimitKb  = problem.MemoryLimitKb,
            FloatTolerance = problem.FloatTolerance,
            AuthorUsername = problem.AuthorUsername,
            TagIds         = problem.Tags.Select(t => t.Id).ToList(),
            Attachments    = problem.Attachments.ToList(),
        };
        return View(model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public IActionResult Edit(int id, CreateProblemViewModel model)
    {
        var problem = _db.Problems.Include(p => p.Tags).FirstOrDefault(p => p.Id == id);
        if (problem is null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["Tags"] = _db.Tags.OrderBy(t => t.Name).ToList();
            model.Id = id;
            return View(model);
        }

        if (!Enum.TryParse<DifficultyLevel>(model.Difficulty, true, out var difficulty))
            difficulty = DifficultyLevel.Easy;

        problem.Title          = model.Title;
        problem.Description    = model.Description;
        problem.Difficulty     = difficulty;
        problem.TimeLimitMs    = model.TimeLimitMs;
        problem.MemoryLimitKb  = model.MemoryLimitKb;
        problem.FloatTolerance = model.FloatTolerance;
        var editTagIds = model.TagIds ?? new List<int>();
        problem.Tags.Clear();
        problem.Tags = _db.Tags.Where(t => editTagIds.Contains(t.Id)).ToList();

        _db.SaveChanges();
        TempData["Flash"] = $"Problem \"{model.Title}\" updated.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // ── Delete (soft delete) ───────────────────────────────────────────────────

    [HttpGet("{id:int}/delete")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var problem = _db.Problems.FirstOrDefault(p => p.Id == id);
        if (problem is null) return NotFound();
        ViewData["Title"] = $"DELETE: {problem.Title.ToUpper()}";
        return View(problem);
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteConfirmed(int id)
    {
        var problem = _db.Problems.FirstOrDefault(p => p.Id == id);
        if (problem is null) return NotFound();

        _repo.SoftDelete(id);
        TempData["Flash"] = "Problem deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── Dropzone attachment upload ─────────────────────────────────────────────

    [HttpPost("{id:int}/attachments")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile? file)
    {
        var problem = _db.Problems.FirstOrDefault(p => p.Id == id);
        if (problem is null) return NotFound();
        if (file is null || file.Length == 0) return BadRequest(new { error = "No file provided." });

        var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".zip", ".txt" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { error = $"File type {ext} is not allowed." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "File size exceeds 10 MB limit." });

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "problems", id.ToString());
        Directory.CreateDirectory(uploadDir);

        var storedName = Guid.NewGuid().ToString() + ext;
        var filePath   = Path.Combine(uploadDir, storedName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var attachment = new Attachment
        {
            ProblemId      = id,
            FileName       = file.FileName,
            StoredFileName = storedName,
            ContentType    = file.ContentType,
            FileSize       = file.Length,
            CreatedAt      = DateTime.UtcNow,
        };
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();

        return Json(new
        {
            id          = attachment.Id,
            fileName    = attachment.FileName,
            contentType = attachment.ContentType,
            fileSize    = attachment.FileSize,
        });
    }

    [HttpGet("{id:int}/attachments")]
    public IActionResult GetAttachments(int id)
    {
        var attachments = _db.Attachments
            .Where(a => a.ProblemId == id)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
        return PartialView("_AttachmentList", attachments);
    }

    [HttpPost("{id:int}/attachments/{attachmentId:int}/delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public IActionResult DeleteAttachment(int id, int attachmentId)
    {
        var attachment = _db.Attachments.FirstOrDefault(a => a.Id == attachmentId && a.ProblemId == id);
        if (attachment is null) return NotFound();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "problems",
            id.ToString(), attachment.StoredFileName);
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

        _db.Attachments.Remove(attachment);
        _db.SaveChanges();
        return Json(new { success = true });
    }
}
