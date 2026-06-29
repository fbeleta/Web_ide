using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("orgs")]
[Authorize(Roles = "Admin,Instructor")]
public class OrganizationController : Controller
{
    private readonly OrganizationRepository _repo;
    private readonly UserRepository _users;

    public OrganizationController(OrganizationRepository repo, UserRepository users)
    {
        _repo  = repo;
        _users = users;
    }

    [Route("")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        ViewData["Title"] = "ORGANIZATIONS";
        return View(_repo.GetAll());
    }

    [Route("{id:int}")]
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var org = _repo.GetById(id);
        if (org is null) return NotFound();
        ViewData["Title"] = org.Name.ToUpper();
        return View(org);
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE ORG";
        return View(new OrganizationCreateModel());
    }

    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(OrganizationCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "CREATE ORG";
            return View(model);
        }
        _repo.Add(new Organization { Name = model.Name, Description = model.Description });
        TempData["Flash"] = $"Organization \"{model.Name}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/edit")]
    [HttpGet, ActionName("Edit")]
    public IActionResult EditGet(int id)
    {
        var org = _repo.GetById(id);
        if (org is null) return NotFound();
        ViewData["Title"] = "EDIT ORG";
        return View(new OrganizationEditModel { Id = org.Id, Name = org.Name, Description = org.Description });
    }

    [Route("{id:int}/edit")]
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(int id, OrganizationEditModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "EDIT ORG";
            return View(model);
        }
        var org = _repo.GetById(id);
        if (org is null) return NotFound();
        org.Name        = model.Name;
        org.Description = model.Description;
        _repo.Update();
        TempData["Flash"] = $"Organization \"{model.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        _repo.SoftDelete(id);
        TempData["Flash"] = "Organization deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/members/add")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddMember(int id, string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            TempData["Flash"] = "Username is required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var matches = _users.Search(username);
        var user = matches.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            TempData["Flash"] = $"User \"{username}\" not found.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var added = _repo.AddMember(id, user.Id);
        TempData["Flash"] = added
            ? $"{user.DisplayName} added to organization."
            : $"{user.DisplayName} is already a member.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [Route("search")]
    [HttpGet]
    public IActionResult Search(string q)
    {
        var results = _repo.Search(q ?? "");
        return Json(results.Select(o => new { id = o.Id, label = o.Name }));
    }
}
