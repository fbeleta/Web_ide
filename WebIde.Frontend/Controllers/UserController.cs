using Microsoft.AspNetCore.Mvc;
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
}
