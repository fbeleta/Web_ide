using Microsoft.AspNetCore.Mvc;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("submissions")]
public class SubmissionController : Controller
{
    private readonly SubmissionRepository _repo;

    public SubmissionController(SubmissionRepository repo) => _repo = repo;

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
}
