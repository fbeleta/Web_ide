using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Authorize(Roles = "Admin,Instructor")]
public class TagController : Controller
{
    private readonly TagRepository _repo;

    public TagController(TagRepository repo) => _repo = repo;

    [AllowAnonymous]
    public IActionResult Index()
    {
        ViewData["Title"] = "TAGS";
        return View(_repo.GetAll());
    }

    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var tag = _repo.GetById(id);
        if (tag is null) return NotFound();
        ViewData["Title"] = tag.Name.ToUpper();
        return View(tag);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE TAG";
        return View(new TagCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TagCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "CREATE TAG";
            return View(model);
        }
        _repo.Add(new Tag { Name = model.Name });
        TempData["Flash"] = $"Tag \"{model.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, ActionName("Edit")]
    public IActionResult EditGet(int id)
    {
        var tag = _repo.GetById(id);
        if (tag is null) return NotFound();
        ViewData["Title"] = "EDIT TAG";
        return View(new TagEditModel { Id = tag.Id, Name = tag.Name });
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(int id, TagEditModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "EDIT TAG";
            return View(model);
        }
        var tag = _repo.GetById(id);
        if (tag is null) return NotFound();
        tag.Name = model.Name;
        _repo.Update();
        TempData["Flash"] = $"Tag \"{model.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        _repo.SoftDelete(id);
        TempData["Flash"] = "Tag deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string q)
    {
        var results = _repo.Search(q ?? "");
        return Json(results.Select(t => new { id = t.Id, label = t.Name }));
    }
}
