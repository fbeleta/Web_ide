using Microsoft.AspNetCore.Mvc;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("submissions/{submissionId:int}/results")]
public class ExecutionResultController : Controller
{
    private readonly ExecutionResultRepository _repo;
    private readonly SubmissionRepository _submissions;

    public ExecutionResultController(ExecutionResultRepository repo, SubmissionRepository submissions)
    {
        _repo = repo;
        _submissions = submissions;
    }

    [Route("{id:int}")]
    public IActionResult Details(int submissionId, int id)
    {
        var result = _repo.GetById(id);
        if (result is null) return NotFound();
        ViewData["Title"] = $"EXECUTION RESULT #{id}";
        ViewData["SubmissionId"] = submissionId;
        return View(result);
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create(int submissionId)
    {
        var submission = _submissions.GetById(submissionId);
        if (submission is null) return NotFound();
        ViewData["Title"] = "CREATE EXECUTION RESULT";
        ViewData["Submission"] = submission;
        return View(new ExecutionResultCreateModel { SubmissionId = submissionId });
    }

    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(int submissionId, ExecutionResultCreateModel model)
    {
        model.SubmissionId = submissionId;
        if (!ModelState.IsValid)
        {
            var submission = _submissions.GetById(submissionId);
            ViewData["Title"] = "CREATE EXECUTION RESULT";
            ViewData["Submission"] = submission;
            return View(model);
        }

        var result = new ExecutionResult
        {
            Stdout         = model.Stdout,
            Stderr         = model.Stderr,
            ExitCode       = model.ExitCode,
            TimedOut       = model.TimedOut,
            MemoryExceeded = model.MemoryExceeded,
        };
        _repo.Add(result);

        // Link the result to the submission
        var sub = _submissions.GetById(submissionId);
        if (sub != null)
        {
            sub.ExecutionResultId = result.Id;
            _submissions.Update();
        }

        TempData["Flash"] = "Execution result recorded.";
        return RedirectToAction("Details", "Submission", new { id = submissionId });
    }
}
