using Microsoft.AspNetCore.Mvc;
using WebIde.Model.Enums;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("problems")]
public class ProblemController : Controller
{
    private readonly ProblemRepository _repo;
    private readonly SubmissionRepository _submissions;

    public ProblemController(ProblemRepository repo, SubmissionRepository submissions)
    {
        _repo = repo;
        _submissions = submissions;
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
}
