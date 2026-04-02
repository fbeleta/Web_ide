using WebIde.Model;
using WebIde.Model.Enums;

// ─── Data Population ─────────────────────────────────────────────────────────

var tagArrays       = new Tag { Id = 1, Name = "Arrays" };
var tagHashMap      = new Tag { Id = 2, Name = "Hash Map" };
var tagSlidingWin   = new Tag { Id = 3, Name = "Sliding Window" };
var tagBinarySearch = new Tag { Id = 4, Name = "Binary Search" };

var problem1 = new Problem
{
    Id = 1,
    Title = "Two Sum",
    Description = "Given an array of integers nums and an integer target, return indices of the two numbers that add up to target.",
    Difficulty = DifficultyLevel.Easy,
    TimeLimitMs = 1000,
    MemoryLimitKb = 65536,
    CreatedAt = new DateTime(2025, 1, 10),
    AuthorUsername = "admin"
};
problem1.Tags.AddRange([tagArrays, tagHashMap]);
problem1.TestCases.AddRange([
    new TestCase { Id = 1, InputArgs = "[2,7,11,15], 9", ExpectedOutput = "[0,1]", IsSample = true,  OrderIndex = 1, Points = 30, Problem = problem1 },
    new TestCase { Id = 2, InputArgs = "[3,2,4], 6",     ExpectedOutput = "[1,2]", IsSample = true,  OrderIndex = 2, Points = 30, Problem = problem1 },
    new TestCase { Id = 3, InputArgs = "[3,3], 6",       ExpectedOutput = "[0,1]", IsSample = false, OrderIndex = 3, Points = 40, Problem = problem1 },
]);
tagArrays.Problems.Add(problem1);
tagHashMap.Problems.Add(problem1);

var problem2 = new Problem
{
    Id = 2,
    Title = "Longest Substring Without Repeating Characters",
    Description = "Given a string s, find the length of the longest substring without duplicate characters.",
    Difficulty = DifficultyLevel.Medium,
    TimeLimitMs = 2000,
    MemoryLimitKb = 65536,
    CreatedAt = new DateTime(2025, 2, 5),
    AuthorUsername = "admin"
};
problem2.Tags.AddRange([tagSlidingWin, tagHashMap]);
problem2.TestCases.AddRange([
    new TestCase { Id = 4, InputArgs = "\"abcabcbb\"", ExpectedOutput = "3", IsSample = true,  OrderIndex = 1, Points = 33, Problem = problem2 },
    new TestCase { Id = 5, InputArgs = "\"bbbbb\"",    ExpectedOutput = "1", IsSample = true,  OrderIndex = 2, Points = 33, Problem = problem2 },
    new TestCase { Id = 6, InputArgs = "\"pwwkew\"",   ExpectedOutput = "3", IsSample = false, OrderIndex = 3, Points = 34, Problem = problem2 },
]);
tagSlidingWin.Problems.Add(problem2);
tagHashMap.Problems.Add(problem2);

var problem3 = new Problem
{
    Id = 3,
    Title = "Median of Two Sorted Arrays",
    Description = "Given two sorted arrays nums1 and nums2, return the median of the two sorted arrays.",
    Difficulty = DifficultyLevel.Hard,
    TimeLimitMs = 3000,
    MemoryLimitKb = 131072,
    CreatedAt = new DateTime(2025, 3, 20),
    AuthorUsername = "admin"
};
problem3.Tags.Add(tagBinarySearch);
problem3.TestCases.AddRange([
    new TestCase { Id = 7,  InputArgs = "[1,3], [2]",   ExpectedOutput = "2.0", IsSample = true,  OrderIndex = 1, Points = 25, Problem = problem3 },
    new TestCase { Id = 8,  InputArgs = "[1,2], [3,4]", ExpectedOutput = "2.5", IsSample = true,  OrderIndex = 2, Points = 25, Problem = problem3 },
    new TestCase { Id = 9,  InputArgs = "[0,0], [0,0]", ExpectedOutput = "0.0", IsSample = false, OrderIndex = 3, Points = 25, Problem = problem3 },
    new TestCase { Id = 10, InputArgs = "[], [1]",      ExpectedOutput = "1.0", IsSample = false, OrderIndex = 4, Points = 25, Problem = problem3 },
]);
tagBinarySearch.Problems.Add(problem3);

var problems = new List<Problem> { problem1, problem2, problem3 };

var user1 = new User { Id = 1, Username = "ana_k",   Email = "ana@example.com",   DisplayName = "Ana Kovač",    Role = UserRole.Student,    RegisteredAt = new DateTime(2025, 9, 1) };
var user2 = new User { Id = 2, Username = "mario_b", Email = "mario@example.com", DisplayName = "Mario Blažić", Role = UserRole.Student,    RegisteredAt = new DateTime(2025, 9, 1) };
var user3 = new User { Id = 3, Username = "prof_hr", Email = "prof@example.com",  DisplayName = "Prof. Horvat", Role = UserRole.Instructor, RegisteredAt = new DateTime(2024, 6, 15) };

var users = new List<User> { user1, user2, user3 };

var org = new Organization
{
    Id = 1,
    Name = "FER Algorithms",
    Description = "Faculty of Electrical Engineering and Computing - Algorithms course"
};
org.Members.AddRange([user1, user2, user3]);
user1.Organizations.Add(org);
user2.Organizations.Add(org);
user3.Organizations.Add(org);

var problemSet = new ProblemSet
{
    Id = 1,
    Title = "Week 1 - Fundamentals",
    Description = "Array manipulation and hashing basics.",
    CreatedAt = new DateTime(2025, 10, 1),
    IsPublic = true,
    OrderIndex = 1,
    Organization = org
};
problemSet.Problems.AddRange(problems);
org.ProblemSets.Add(problemSet);

Submission MakeSubmission(int id, User user, Problem problem, string code,
    SubmissionStatus status, int score, int wallMs, int memKb, string stdout) =>
    new Submission
    {
        Id = id, User = user, Problem = problem,
        SourceCode = code, Language = "cpp",
        Status = status, SubmittedAt = DateTime.Now.AddMinutes(-id * 3),
        Score = score, WallTimeMs = wallMs, PeakMemoryKb = memKb,
        ExecutionResult = new ExecutionResult
        {
            Id = id, Stdout = stdout, Stderr = "",
            ExitCode = status == SubmissionStatus.CompileError ? 1 : 0,
            TimedOut = status == SubmissionStatus.TimeLimitExceeded,
            MemoryExceeded = status == SubmissionStatus.MemoryLimitExceeded
        }
    };

var submissions = new List<Submission>
{
    MakeSubmission(1, user1, problem1, "// correct two sum",         SubmissionStatus.Accepted,           100, 45,   2048, "[0,1]"),
    MakeSubmission(2, user1, problem2, "// sliding window solution", SubmissionStatus.Accepted,           100, 120,  4096, "3"),
    MakeSubmission(3, user1, problem3, "// binary search attempt",   SubmissionStatus.WrongAnswer,         60, 200,  8192, "2.0"),
    MakeSubmission(4, user2, problem1, "// brute force O(n^2)",      SubmissionStatus.Accepted,           100, 980,  2048, "[0,1]"),
    MakeSubmission(5, user2, problem2, "// naive solution TLE",      SubmissionStatus.TimeLimitExceeded,    0, 2001, 4096, ""),
    MakeSubmission(6, user2, problem3, "// correct binary search",   SubmissionStatus.Accepted,           100, 55,   4096, "2.5"),
    MakeSubmission(7, user3, problem1, "// optimal hash map",        SubmissionStatus.Accepted,           100, 32,   1024, "[0,1]"),
    MakeSubmission(8, user3, problem3, "// compile error",           SubmissionStatus.CompileError,         0, 0,      0, ""),
};

foreach (var s in submissions)
{
    s.User.Submissions.Add(s);
    s.Problem.Submissions.Add(s);
}

Console.WriteLine("=== WebIde Object Model - Lab 1 ===\n");
Console.WriteLine($"Problems: {problems.Count} | Users: {users.Count} | Submissions: {submissions.Count}");
Console.WriteLine($"Organization: {org.Name} | ProblemSet: {problemSet.Title}\n");

// ─── LINQ Queries ─────────────────────────────────────────────────────────────

Console.WriteLine("─── 1. Easy problems ───────────────────────────────────────────");
var easyProblems = problems
    .Where(p => p.Difficulty == DifficultyLevel.Easy)
    .ToList();
foreach (var p in easyProblems)
    Console.WriteLine($"  {p.Title} ({p.Difficulty})");

Console.WriteLine("\n─── 2. Accepted submissions for ana_k (newest first) ────────────");
var anaAccepted = user1.Submissions
    .Where(s => s.Status == SubmissionStatus.Accepted)
    .OrderByDescending(s => s.SubmittedAt)
    .ToList();
foreach (var s in anaAccepted)
    Console.WriteLine($"  [{s.SubmittedAt:HH:mm:ss}] {s.Problem.Title} - score: {s.Score}");

Console.WriteLine("\n─── 3. Problems tagged 'Arrays' ─────────────────────────────────");
var arrayProblems = problems
    .Where(p => p.Tags.Any(t => t.Name == "Arrays"))
    .ToList();
foreach (var p in arrayProblems)
    Console.WriteLine($"  {p.Title}");

Console.WriteLine("\n─── 4. Top 3 scorers on 'Two Sum' ──────────────────────────────");
var topScorers = problem1.Submissions
    .OrderByDescending(s => s.Score)
    .ThenBy(s => s.WallTimeMs)
    .Take(3)
    .ToList();
foreach (var s in topScorers)
    Console.WriteLine($"  {s.User.DisplayName}: score={s.Score}, time={s.WallTimeMs}ms");

Console.WriteLine("\n─── 5. Submission count per problem ─────────────────────────────");
var submissionCounts = problems
    .Select(p => new { p.Title, Count = p.Submissions.Count })
    .OrderByDescending(x => x.Count)
    .ToList();
foreach (var x in submissionCounts)
    Console.WriteLine($"  {x.Title}: {x.Count} submissions");

Console.WriteLine("\n─── 6. Problems with no accepted submissions ────────────────────");
var noAccepted = problems
    .Where(p => !p.Submissions.Any(s => s.Status == SubmissionStatus.Accepted))
    .ToList();
if (noAccepted.Count == 0)
    Console.WriteLine("  All problems have at least one accepted submission.");
else
    foreach (var p in noAccepted)
        Console.WriteLine($"  {p.Title}");

Console.WriteLine("\n─── 7. Sample test cases for 'Median of Two Sorted Arrays' ──────");
var sampleCases = problem3.TestCases
    .Where(tc => tc.IsSample)
    .OrderBy(tc => tc.OrderIndex)
    .ToList();
foreach (var tc in sampleCases)
    Console.WriteLine($"  Input: {tc.InputArgs} -> Expected: {tc.ExpectedOutput} ({tc.Points} pts)");

// ─── Async Demo ───────────────────────────────────────────────────────────────

Console.WriteLine("\n─── Async: simulate parallel code execution ─────────────────────");

static async Task SimulateCodeExecutionAsync(Submission submission)
{
    int compileMs = new Random().Next(200, 600);
    int runMs     = new Random().Next(100, 400);
    Console.WriteLine($"  [{submission.User.Username}] Compiling '{submission.Problem.Title}'...");
    await Task.Delay(compileMs);
    Console.WriteLine($"  [{submission.User.Username}] Running '{submission.Problem.Title}'...");
    await Task.Delay(runMs);
    Console.WriteLine($"  [{submission.User.Username}] Done ~{compileMs + runMs}ms -> {submission.Status}");
}

var parallelSubmissions = new[] { submissions[0], submissions[3], submissions[6] };

Console.WriteLine("  Launching 3 submissions in parallel via Task.WhenAll:");
await Task.WhenAll(parallelSubmissions.Select(SimulateCodeExecutionAsync));

Console.WriteLine("\nAll tasks completed.");
