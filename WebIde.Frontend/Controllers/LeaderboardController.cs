using Microsoft.AspNetCore.Mvc;
using WebIde.Model.Enums;
using WebIde.Web.Models;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

public class LeaderboardController : Controller
{
    private readonly UserRepository _users;

    public LeaderboardController(UserRepository users) => _users = users;

    public IActionResult Index()
    {
        var ranked = _users.GetAll()
            .Select(u => new
            {
                User = u,
                SolvedCount = u.Submissions.Count(s => s.Status == SubmissionStatus.Accepted),
                Score = u.Submissions.Where(s => s.Status == SubmissionStatus.Accepted).Sum(s => s.Score)
            })
            .OrderByDescending(x => x.SolvedCount)
            .ThenByDescending(x => x.Score)
            .Select((x, i) => new LeaderboardEntry(i + 1, x.User, x.SolvedCount, x.Score))
            .ToList();

        ViewData["Title"] = "LEADERBOARD";
        return View(ranked);
    }
}
