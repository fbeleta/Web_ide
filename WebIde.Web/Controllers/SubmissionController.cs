using Microsoft.AspNetCore.Mvc;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

public class SubmissionController : Controller
{
    private readonly SubmissionRepository _repo;

    public SubmissionController(SubmissionRepository repo) => _repo = repo;

    public IActionResult Index()
    {
        ViewData["Title"] = "SUBMISSIONS";
        return View(_repo.GetAll());
    }

    public IActionResult Details(int id)
    {
        var submission = _repo.GetById(id);
        if (submission is null) return NotFound();
        ViewData["Title"] = $"SUBMISSION #{id}";
        return View(submission);
    }
}
