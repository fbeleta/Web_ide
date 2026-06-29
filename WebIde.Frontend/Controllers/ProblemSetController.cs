using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Authorize(Roles = "Admin,Instructor")]
public class ProblemSetController : Controller
{
    private readonly ProblemSetRepository _repo;
    private readonly OrganizationRepository _orgs;

    public ProblemSetController(ProblemSetRepository repo, OrganizationRepository orgs)
    {
        _repo = repo;
        _orgs = orgs;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        ViewData["Title"] = "PROBLEM SETS";
        return View(_repo.GetAll());
    }

    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var ps = _repo.GetById(id);
        if (ps is null) return NotFound();
        ViewData["Title"] = ps.Title.ToUpper();
        return View(ps);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE PROBLEM SET";
        return View(new ProblemSetCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProblemSetCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "CREATE PROBLEM SET";
            return View(model);
        }
        var org = _orgs.GetById(model.OrganizationId);
        if (org is null)
        {
            ModelState.AddModelError(nameof(model.OrganizationId), "Selected organization does not exist.");
            ViewData["Title"] = "CREATE PROBLEM SET";
            return View(model);
        }

        _repo.Add(new ProblemSet
        {
            Title          = model.Title,
            Description    = model.Description,
            CreatedAt      = model.CreatedAt,
            IsPublic       = model.IsPublic,
            OrderIndex     = model.OrderIndex,
            OrganizationId = model.OrganizationId,
        });
        TempData["Flash"] = $"Problem set \"{model.Title}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, ActionName("Edit")]
    public IActionResult EditGet(int id)
    {
        var ps = _repo.GetById(id);
        if (ps is null) return NotFound();
        ViewData["Title"] = "EDIT PROBLEM SET";
        return View(new ProblemSetEditModel
        {
            Id             = ps.Id,
            Title          = ps.Title,
            Description    = ps.Description,
            CreatedAt      = ps.CreatedAt,
            IsPublic       = ps.IsPublic,
            OrderIndex     = ps.OrderIndex,
            OrganizationId = ps.OrganizationId,
            OrganizationName = ps.Organization?.Name ?? "",
        });
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(int id, ProblemSetEditModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "EDIT PROBLEM SET";
            return View(model);
        }
        var ps = _repo.GetById(id);
        if (ps is null) return NotFound();

        ps.Title          = model.Title;
        ps.Description    = model.Description;
        ps.CreatedAt      = model.CreatedAt;
        ps.IsPublic       = model.IsPublic;
        ps.OrderIndex     = model.OrderIndex;
        ps.OrganizationId = model.OrganizationId;
        _repo.Update();
        TempData["Flash"] = $"Problem set \"{model.Title}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        _repo.SoftDelete(id);
        TempData["Flash"] = "Problem set deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string q)
    {
        var results = _repo.Search(q ?? "");
        return Json(results.Select(ps => new { id = ps.Id, label = ps.Title }));
    }
}
