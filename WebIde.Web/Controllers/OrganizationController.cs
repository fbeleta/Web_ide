using Microsoft.AspNetCore.Mvc;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

public class OrganizationController : Controller
{
    private readonly OrganizationRepository _repo;

    public OrganizationController(OrganizationRepository repo) => _repo = repo;

    public IActionResult Index()
    {
        ViewData["Title"] = "ORGANIZATIONS";
        return View(_repo.GetAll());
    }

    public IActionResult Details(int id)
    {
        var org = _repo.GetById(id);
        if (org is null) return NotFound();
        ViewData["Title"] = org.Name.ToUpper();
        return View(org);
    }
}
