using WebIde.Model;
using WebIde.Model.Enums;

namespace WebIde.Web.Repositories;

// Static seed data ported and expanded from WebIde.Console/Program.cs (Lab 1).
// Provides the shared in-memory graph used by all mock repositories.
public static class MockData
{
    public static readonly List<Tag> Tags;
    public static readonly List<Problem> Problems;
    public static readonly List<User> Users;
    public static readonly List<Organization> Organizations;
    public static readonly List<ProblemSet> ProblemSets;
    public static readonly List<Submission> Submissions;

    static MockData()
    {
        // ── Tags ─────────────────────────────────────────────────────────────
        var tagArrays       = new Tag { Id = 1, Name = "Arrays" };
        var tagHashMap      = new Tag { Id = 2, Name = "Hash Map" };
        var tagSlidingWin   = new Tag { Id = 3, Name = "Sliding Window" };
        var tagBinarySearch = new Tag { Id = 4, Name = "Binary Search" };
        var tagDp           = new Tag { Id = 5, Name = "Dynamic Programming" };
        var tagGraph        = new Tag { Id = 6, Name = "Graph" };

        Tags = [tagArrays, tagHashMap, tagSlidingWin, tagBinarySearch, tagDp, tagGraph];

        // ── Problems ─────────────────────────────────────────────────────────
        var p1 = new Problem
        {
            Id = 1, Title = "Two Sum",
            Description = "Given an array of integers nums and an integer target, return indices of the two numbers that add up to target.",
            Difficulty = DifficultyLevel.Easy,
            TimeLimitMs = 1000, MemoryLimitKb = 65536,
            CreatedAt = new DateTime(2025, 1, 10), AuthorUsername = "admin"
        };
        p1.Tags.AddRange([tagArrays, tagHashMap]);
        p1.TestCases.AddRange([
            new TestCase { Id = 1,  InputArgs = "[2,7,11,15], 9", ExpectedOutput = "[0,1]", IsSample = true,  OrderIndex = 1, Points = 30, Problem = p1 },
            new TestCase { Id = 2,  InputArgs = "[3,2,4], 6",     ExpectedOutput = "[1,2]", IsSample = true,  OrderIndex = 2, Points = 30, Problem = p1 },
            new TestCase { Id = 3,  InputArgs = "[3,3], 6",       ExpectedOutput = "[0,1]", IsSample = false, OrderIndex = 3, Points = 40, Problem = p1 },
        ]);
        tagArrays.Problems.Add(p1);
        tagHashMap.Problems.Add(p1);

        var p2 = new Problem
        {
            Id = 2, Title = "Longest Substring Without Repeating Characters",
            Description = "Given a string s, find the length of the longest substring without duplicate characters.",
            Difficulty = DifficultyLevel.Medium,
            TimeLimitMs = 2000, MemoryLimitKb = 65536,
            CreatedAt = new DateTime(2025, 2, 5), AuthorUsername = "admin"
        };
        p2.Tags.AddRange([tagSlidingWin, tagHashMap]);
        p2.TestCases.AddRange([
            new TestCase { Id = 4, InputArgs = "\"abcabcbb\"", ExpectedOutput = "3", IsSample = true,  OrderIndex = 1, Points = 33, Problem = p2 },
            new TestCase { Id = 5, InputArgs = "\"bbbbb\"",    ExpectedOutput = "1", IsSample = true,  OrderIndex = 2, Points = 33, Problem = p2 },
            new TestCase { Id = 6, InputArgs = "\"pwwkew\"",   ExpectedOutput = "3", IsSample = false, OrderIndex = 3, Points = 34, Problem = p2 },
        ]);
        tagSlidingWin.Problems.Add(p2);
        tagHashMap.Problems.Add(p2);

        var p3 = new Problem
        {
            Id = 3, Title = "Median of Two Sorted Arrays",
            Description = "Given two sorted arrays nums1 and nums2, return the median of the two sorted arrays. The overall run time complexity should be O(log(m+n)).",
            Difficulty = DifficultyLevel.Hard,
            TimeLimitMs = 3000, MemoryLimitKb = 131072,
            CreatedAt = new DateTime(2025, 3, 20), AuthorUsername = "admin"
        };
        p3.Tags.Add(tagBinarySearch);
        p3.TestCases.AddRange([
            new TestCase { Id = 7,  InputArgs = "[1,3], [2]",   ExpectedOutput = "2.0", IsSample = true,  OrderIndex = 1, Points = 25, Problem = p3 },
            new TestCase { Id = 8,  InputArgs = "[1,2], [3,4]", ExpectedOutput = "2.5", IsSample = true,  OrderIndex = 2, Points = 25, Problem = p3 },
            new TestCase { Id = 9,  InputArgs = "[0,0], [0,0]", ExpectedOutput = "0.0", IsSample = false, OrderIndex = 3, Points = 25, Problem = p3 },
            new TestCase { Id = 10, InputArgs = "[], [1]",      ExpectedOutput = "1.0", IsSample = false, OrderIndex = 4, Points = 25, Problem = p3 },
        ]);
        tagBinarySearch.Problems.Add(p3);

        // Extra problems for richer demo
        var p4 = new Problem
        {
            Id = 4, Title = "Climbing Stairs",
            Description = "You are climbing a staircase. It takes n steps to reach the top. Each time you can climb 1 or 2 steps. In how many distinct ways can you climb to the top?",
            Difficulty = DifficultyLevel.Easy,
            TimeLimitMs = 1000, MemoryLimitKb = 32768,
            CreatedAt = new DateTime(2025, 4, 1), AuthorUsername = "prof_hr"
        };
        p4.Tags.Add(tagDp);
        p4.TestCases.AddRange([
            new TestCase { Id = 11, InputArgs = "2", ExpectedOutput = "2", IsSample = true,  OrderIndex = 1, Points = 50, Problem = p4 },
            new TestCase { Id = 12, InputArgs = "3", ExpectedOutput = "3", IsSample = true,  OrderIndex = 2, Points = 50, Problem = p4 },
        ]);
        tagDp.Problems.Add(p4);

        var p5 = new Problem
        {
            Id = 5, Title = "Course Schedule",
            Description = "There are numCourses courses you have to take. Some courses have prerequisites. Given the total number of courses and a list of prerequisite pairs, determine if it is possible to finish all courses.",
            Difficulty = DifficultyLevel.Medium,
            TimeLimitMs = 2000, MemoryLimitKb = 65536,
            CreatedAt = new DateTime(2025, 5, 15), AuthorUsername = "prof_hr"
        };
        p5.Tags.Add(tagGraph);
        p5.TestCases.AddRange([
            new TestCase { Id = 13, InputArgs = "2, [[1,0]]",         ExpectedOutput = "true",  IsSample = true,  OrderIndex = 1, Points = 50, Problem = p5 },
            new TestCase { Id = 14, InputArgs = "2, [[1,0],[0,1]]",   ExpectedOutput = "false", IsSample = true,  OrderIndex = 2, Points = 50, Problem = p5 },
        ]);
        tagGraph.Problems.Add(p5);

        Problems = [p1, p2, p3, p4, p5];

        // ── Users ─────────────────────────────────────────────────────────────
        var u1 = new User { Id = 1, Username = "ana_k",   Email = "ana@example.com",   DisplayName = "Ana Kovač",    Role = UserRole.Student,    RegisteredAt = new DateTime(2025, 9, 1) };
        var u2 = new User { Id = 2, Username = "mario_b", Email = "mario@example.com", DisplayName = "Mario Blažić", Role = UserRole.Student,    RegisteredAt = new DateTime(2025, 9, 1) };
        var u3 = new User { Id = 3, Username = "prof_hr", Email = "prof@example.com",  DisplayName = "Prof. Horvat", Role = UserRole.Instructor, RegisteredAt = new DateTime(2024, 6, 15) };
        var u4 = new User { Id = 4, Username = "admin",   Email = "admin@webide.io",   DisplayName = "Admin",        Role = UserRole.Admin,      RegisteredAt = new DateTime(2024, 1, 1) };

        Users = [u1, u2, u3, u4];

        // ── Organizations ─────────────────────────────────────────────────────
        var org1 = new Organization { Id = 1, Name = "FER Algorithms", Description = "Faculty of Electrical Engineering and Computing — Algorithms course. Weekly problem sets and graded contests." };
        org1.Members.AddRange([u1, u2, u3]);
        u1.Organizations.Add(org1);
        u2.Organizations.Add(org1);
        u3.Organizations.Add(org1);

        var org2 = new Organization { Id = 2, Name = "Open Source Club", Description = "Student-run open source club. Practice problems, hackathons, and community challenges." };
        org2.Members.AddRange([u1, u4]);
        u1.Organizations.Add(org2);
        u4.Organizations.Add(org2);

        Organizations = [org1, org2];

        // ── Problem Sets ──────────────────────────────────────────────────────
        var ps1 = new ProblemSet
        {
            Id = 1, Title = "Week 1 — Fundamentals",
            Description = "Array manipulation and hashing basics.",
            CreatedAt = new DateTime(2025, 10, 1), IsPublic = true, OrderIndex = 1,
            Organization = org1
        };
        ps1.Problems.AddRange([p1, p2, p3]);
        org1.ProblemSets.Add(ps1);

        var ps2 = new ProblemSet
        {
            Id = 2, Title = "Advanced Algorithms",
            Description = "Dynamic programming, graph traversal, and advanced search techniques.",
            CreatedAt = new DateTime(2025, 11, 1), IsPublic = false, OrderIndex = 2,
            Organization = org1
        };
        ps2.Problems.AddRange([p4, p5]);
        org1.ProblemSets.Add(ps2);

        var ps3 = new ProblemSet
        {
            Id = 3, Title = "OSC Sprint #1",
            Description = "Quick 2-hour sprint with easy and medium problems. Open to all club members.",
            CreatedAt = new DateTime(2025, 12, 1), IsPublic = true, OrderIndex = 1,
            Organization = org2
        };
        ps3.Problems.AddRange([p1, p4]);
        org2.ProblemSets.Add(ps3);

        ProblemSets = [ps1, ps2, ps3];

        // ── Submissions ───────────────────────────────────────────────────────
        static Submission MakeSub(int id, User user, Problem problem, string code,
            SubmissionStatus status, int score, int wallMs, int memKb, string stdout) =>
            new Submission
            {
                Id = id, User = user, Problem = problem,
                SourceCode = code, Language = "cpp",
                Status = status, SubmittedAt = DateTime.Now.AddMinutes(-id * 5),
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
            MakeSub(1, u1, p1, "// correct two sum\nmap<int,int> seen;\nfor(int i=0;i<n;i++){\n  int d=target-nums[i];\n  if(seen.count(d)) return {seen[d],i};\n  seen[nums[i]]=i;\n}",
                SubmissionStatus.Accepted,           100,  45,  2048, "[0,1]"),
            MakeSub(2, u1, p2, "// sliding window solution\nint l=0,res=0;\nunordered_set<char>s;\nfor(char c:str){\n  while(s.count(c))s.erase(str[l++]);\n  s.insert(c);res=max(res,(int)s.size());\n}",
                SubmissionStatus.Accepted,           100, 120,  4096, "3"),
            MakeSub(3, u1, p3, "// binary search attempt",
                SubmissionStatus.WrongAnswer,         60, 200,  8192, "2.0"),
            MakeSub(4, u2, p1, "// brute force O(n^2)\nfor(int i=0;i<n;i++)\n  for(int j=i+1;j<n;j++)\n    if(nums[i]+nums[j]==target) return {i,j};",
                SubmissionStatus.Accepted,           100, 980,  2048, "[0,1]"),
            MakeSub(5, u2, p2, "// naive solution TLE",
                SubmissionStatus.TimeLimitExceeded,    0, 2001, 4096, ""),
            MakeSub(6, u2, p3, "// correct binary search\ndouble findMedianSortedArrays(vector<int>&a,vector<int>&b){...}",
                SubmissionStatus.Accepted,           100,  55,  4096, "2.5"),
            MakeSub(7, u3, p1, "// optimal hash map — O(n)",
                SubmissionStatus.Accepted,           100,  32,  1024, "[0,1]"),
            MakeSub(8, u3, p3, "// compile error\n#include<vectorr>",
                SubmissionStatus.CompileError,          0,   0,     0, ""),
        };

        foreach (var s in submissions)
        {
            s.User.Submissions.Add(s);
            s.Problem.Submissions.Add(s);
        }

        Submissions = submissions;
    }
}
