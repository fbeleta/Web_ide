using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebIde.Model.Enums;
using WebIde.Web.Models;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

public class HomeController : Controller
{
    private readonly ProblemRepository _problems;
    private readonly UserRepository _users;
    private readonly SubmissionRepository _submissions;

    public HomeController(ProblemRepository problems, UserRepository users, SubmissionRepository submissions)
    {
        _problems = problems;
        _users = users;
        _submissions = submissions;
    }

    public IActionResult Index()
    {
        var allSubmissions = _submissions.GetAll();
        var vm = new HomeViewModel
        {
            TotalProblems = _problems.GetAll().Count,
            TotalUsers = _users.GetAll().Count,
            TotalSubmissions = allSubmissions.Count,
            AcceptedSubmissions = allSubmissions.Count(s => s.Status == SubmissionStatus.Accepted),
            FeaturedProblems = _problems.GetAll().Take(3).ToList(),
            TopUsers = _users.GetAll()
                .Select(u => (u, u.Submissions.Count(s => s.Status == SubmissionStatus.Accepted)))
                .OrderByDescending(x => x.Item2)
                .Take(3)
                .ToList()
        };
        ViewData["Title"] = "HOME";
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
