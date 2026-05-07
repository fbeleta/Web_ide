using WebIde.Model;

namespace WebIde.Web.Models;

public record LeaderboardEntry(int Rank, User User, int SolvedCount, int Score);
