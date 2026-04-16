using Microsoft.AspNetCore.Mvc;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

public class ProblemController : Controller
{
    private readonly ProblemRepository _repo;
    private readonly SubmissionRepository _submissions;

    public ProblemController(ProblemRepository repo, SubmissionRepository submissions)
    {
        _repo = repo;
        _submissions = submissions;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "PROBLEMS";
        return View(_repo.GetAll());
    }

    public IActionResult Details(int id)
    {
        var problem = _repo.GetById(id);
        if (problem is null) return NotFound();
        ViewData["Title"] = problem.Title.ToUpper();
        return View(problem);
    }
}
