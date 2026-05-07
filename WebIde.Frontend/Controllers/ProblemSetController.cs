using Microsoft.AspNetCore.Mvc;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

public class ProblemSetController : Controller
{
    // Injected by ASP.NET DI (registered as Singleton in Program.cs).
    // The underscore prefix marks it as a private field, distinct from constructor params.
    private readonly ProblemSetRepository _repo;

    public ProblemSetController(ProblemSetRepository repo) => _repo = repo;

    public IActionResult Index()
    {
        ViewData["Title"] = "PROBLEM SETS";
        return View(_repo.GetAll());
    }

    public IActionResult Details(int id)
    {
        var ps = _repo.GetById(id);
        if (ps is null) return NotFound();
        ViewData["Title"] = ps.Title.ToUpper();
        return View(ps);
    }
}
