using Microsoft.AspNetCore.Mvc;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

public class TagController : Controller
{
    private readonly TagRepository _repo;

    public TagController(TagRepository repo) => _repo = repo;

    public IActionResult Index()
    {
        ViewData["Title"] = "TAGS";
        return View(_repo.GetAll());
    }

    public IActionResult Details(int id)
    {
        var tag = _repo.GetById(id);
        if (tag is null) return NotFound();
        ViewData["Title"] = tag.Name.ToUpper();
        return View(tag);
    }
}
