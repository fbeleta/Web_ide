using Microsoft.AspNetCore.Mvc;
using WebIde.Model.Enums;
using WebIde.Web.Models;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("leaderboard")]
public class LeaderboardController : Controller
{
    private readonly UserRepository _users;

    public LeaderboardController(UserRepository users) => _users = users;

    [Route("")]
    public IActionResult Index(string? sort)
    {
        var entries = _users.GetAll()
            .Select(u => new
            {
                User = u,
                SolvedCount = u.Submissions.Count(s => s.Status == SubmissionStatus.Accepted),
                Score = u.Submissions.Where(s => s.Status == SubmissionStatus.Accepted).Sum(s => s.Score)
            });

        entries = sort switch
        {
            "score" => entries.OrderByDescending(x => x.Score).ThenByDescending(x => x.SolvedCount),
            _       => entries.OrderByDescending(x => x.SolvedCount).ThenByDescending(x => x.Score),
        };

        var ranked = entries
            .Select((x, i) => new LeaderboardEntry(i + 1, x.User, x.SolvedCount, x.Score))
            .ToList();

        ViewData["Title"] = "LEADERBOARD";
        ViewData["sort"] = sort ?? "solved";
        return View(ranked);
    }
}
