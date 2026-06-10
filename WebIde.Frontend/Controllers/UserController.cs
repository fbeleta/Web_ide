using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("users")]
public class UserController : Controller
{
    private readonly UserRepository _repo;

    public UserController(UserRepository repo) => _repo = repo;

    [Route("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "USERS";
        return View(_repo.GetAll());
    }

    [Route("{id:int}")]
    public IActionResult Details(int id)
    {
        var user = _repo.GetById(id);
        if (user is null) return NotFound();
        ViewData["Title"] = user.Username.ToUpper();
        return View(user);
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "CREATE USER";
        return View(new UserCreateModel());
    }

    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "CREATE USER";
            return View(model);
        }
        _repo.Add(new User
        {
            Username    = model.Username,
            Email       = model.Email,
            DisplayName = model.DisplayName,
            Role        = model.Role,
            RegisteredAt = model.RegisteredAt,
        });
        TempData["Flash"] = $"User \"{model.Username}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/edit")]
    [HttpGet, ActionName("Edit")]
    [Authorize]
    public IActionResult EditGet(int id)
    {
        var user = _repo.GetById(id);
        if (user is null) return NotFound();
        ViewData["Title"] = "EDIT USER";
        return View(new UserEditModel
        {
            Id          = user.Id,
            Username    = user.Username,
            Email       = user.Email,
            DisplayName = user.DisplayName,
            Role        = user.Role,
            RegisteredAt = user.RegisteredAt,
        });
    }

    [Route("{id:int}/edit")]
    [HttpPost, ActionName("Edit")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(int id, UserEditModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "EDIT USER";
            return View(model);
        }
        var user = _repo.GetById(id);
        if (user is null) return NotFound();
        user.Username    = model.Username;
        user.Email       = model.Email;
        user.DisplayName = model.DisplayName;
        user.Role        = model.Role;
        user.RegisteredAt = model.RegisteredAt;
        _repo.Update();
        TempData["Flash"] = $"User \"{model.Username}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [Route("{id:int}/delete")]
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _repo.SoftDelete(id);
        TempData["Flash"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Route("search")]
    [HttpGet]
    public IActionResult Search(string q)
    {
        var results = _repo.Search(q ?? "");
        return Json(results.Select(u => new { id = u.Id, label = u.Username + " — " + u.DisplayName }));
    }
}
