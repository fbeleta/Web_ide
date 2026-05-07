using Microsoft.AspNetCore.Mvc;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("submissions")]
public class SubmissionController : Controller
{
    private readonly SubmissionRepository _repo;

    public SubmissionController(SubmissionRepository repo) => _repo = repo;

    [Route("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "SUBMISSIONS";
        return View(_repo.GetAll());
    }

    [Route("{id:int}")]
    public IActionResult Details(int id)
    {
        var submission = _repo.GetById(id);
        if (submission is null) return NotFound();
        ViewData["Title"] = $"SUBMISSION #{id}";
        return View(submission);
    }

    [Route("~/problems/{problemId:int}/submissions")]
    public IActionResult ByProblem(int problemId)
    {
        ViewData["Title"] = $"SUBMISSIONS FOR PROBLEM #{problemId}";
        return View("Index", _repo.GetByProblem(problemId));
    }
}
