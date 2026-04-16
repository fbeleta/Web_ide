using WebIde.Model;

namespace WebIde.Web.Models;

public class HomeViewModel
{
    public int TotalProblems { get; set; }
    public int TotalUsers { get; set; }
    public int TotalSubmissions { get; set; }
    public int AcceptedSubmissions { get; set; }
    public double AcceptanceRate => TotalSubmissions == 0 ? 0 : Math.Round((double)AcceptedSubmissions / TotalSubmissions * 100, 1);
    public List<Problem> FeaturedProblems { get; set; } = [];
    public List<(User User, int SolvedCount)> TopUsers { get; set; } = [];
}
