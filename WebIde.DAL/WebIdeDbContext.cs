using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebIde.Model;
using WebIde.Model.Enums;

namespace WebIde.DAL;

public class WebIdeDbContext : IdentityDbContext<AppUser>
{
    public WebIdeDbContext(DbContextOptions<WebIdeDbContext> options) : base(options) { }

    public DbSet<Problem> Problems { get; set; }
    public DbSet<User> DomainUsers { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TestCase> TestCases { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<ExecutionResult> ExecutionResults { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<ProblemSet> ProblemSets { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // sets up Identity tables

        // N-N: Problem <-> Tag
        modelBuilder.Entity<Problem>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Problems)
            .UsingEntity("ProblemTag", j => j.ToTable("ProblemTags"));

        // N-N: Problem <-> ProblemSet
        modelBuilder.Entity<ProblemSet>()
            .HasMany(ps => ps.Problems)
            .WithMany()
            .UsingEntity("ProblemProblemSet", j => j.ToTable("ProblemSetProblems"));

        // N-N: Organization <-> User (Members)
        modelBuilder.Entity<Organization>()
            .HasMany(o => o.Members)
            .WithMany(u => u.Organizations)
            .UsingEntity("OrganizationUser", j => j.ToTable("OrganizationMembers"));

        // ── Seed data ─────────────────────────────────────────────────────────

        // Tags
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Arrays" },
            new Tag { Id = 2, Name = "Hash Map" },
            new Tag { Id = 3, Name = "Sliding Window" },
            new Tag { Id = 4, Name = "Binary Search" },
            new Tag { Id = 5, Name = "Dynamic Programming" },
            new Tag { Id = 6, Name = "Graph" }
        );

        // Users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "ana_k",   Email = "ana@example.com",   DisplayName = "Ana Kovač",    Role = UserRole.Student,    RegisteredAt = new DateTime(2025, 9, 1) },
            new User { Id = 2, Username = "mario_b", Email = "mario@example.com", DisplayName = "Mario Blažić", Role = UserRole.Student,    RegisteredAt = new DateTime(2025, 9, 1) },
            new User { Id = 3, Username = "prof_hr", Email = "prof@example.com",  DisplayName = "Prof. Horvat", Role = UserRole.Instructor, RegisteredAt = new DateTime(2024, 6, 15) },
            new User { Id = 4, Username = "admin",   Email = "admin@webide.io",   DisplayName = "Admin",        Role = UserRole.Admin,      RegisteredAt = new DateTime(2024, 1, 1) }
        );

        // Problems
        modelBuilder.Entity<Problem>().HasData(
            new Problem { Id = 1, Title = "Two Sum", Description = "Given an array of integers nums and an integer target, return indices of the two numbers that add up to target.", Difficulty = DifficultyLevel.Easy,   TimeLimitMs = 1000, MemoryLimitKb = 65536,  CreatedAt = new DateTime(2025, 1, 10),  AuthorUsername = "admin" },
            new Problem { Id = 2, Title = "Longest Substring Without Repeating Characters", Description = "Given a string s, find the length of the longest substring without duplicate characters.", Difficulty = DifficultyLevel.Medium, TimeLimitMs = 2000, MemoryLimitKb = 65536,  CreatedAt = new DateTime(2025, 2, 5),   AuthorUsername = "admin" },
            new Problem { Id = 3, Title = "Median of Two Sorted Arrays", Description = "Given two sorted arrays nums1 and nums2, return the median of the two sorted arrays. The overall run time complexity should be O(log(m+n)).", Difficulty = DifficultyLevel.Hard,   TimeLimitMs = 3000, MemoryLimitKb = 131072, CreatedAt = new DateTime(2025, 3, 20),  AuthorUsername = "admin" },
            new Problem { Id = 4, Title = "Climbing Stairs", Description = "You are climbing a staircase. It takes n steps to reach the top. Each time you can climb 1 or 2 steps. In how many distinct ways can you climb to the top?", Difficulty = DifficultyLevel.Easy,   TimeLimitMs = 1000, MemoryLimitKb = 32768,  CreatedAt = new DateTime(2025, 4, 1),   AuthorUsername = "prof_hr" },
            new Problem { Id = 5, Title = "Course Schedule", Description = "There are numCourses courses you have to take. Some courses have prerequisites. Given the total number of courses and a list of prerequisite pairs, determine if it is possible to finish all courses.", Difficulty = DifficultyLevel.Medium, TimeLimitMs = 2000, MemoryLimitKb = 65536,  CreatedAt = new DateTime(2025, 5, 15),  AuthorUsername = "prof_hr" }
        );

        // Problem <-> Tag (join table seed)
        modelBuilder.Entity("ProblemTag").HasData(
            new { ProblemsId = 1, TagsId = 1 },
            new { ProblemsId = 1, TagsId = 2 },
            new { ProblemsId = 2, TagsId = 3 },
            new { ProblemsId = 2, TagsId = 2 },
            new { ProblemsId = 3, TagsId = 4 },
            new { ProblemsId = 4, TagsId = 5 },
            new { ProblemsId = 5, TagsId = 6 }
        );

        // TestCases
        modelBuilder.Entity<TestCase>().HasData(
            new TestCase { Id = 1,  ProblemId = 1, InputArgs = "[2,7,11,15], 9", ExpectedOutput = "[0,1]", IsSample = true,  OrderIndex = 1, Points = 30 },
            new TestCase { Id = 2,  ProblemId = 1, InputArgs = "[3,2,4], 6",     ExpectedOutput = "[1,2]", IsSample = true,  OrderIndex = 2, Points = 30 },
            new TestCase { Id = 3,  ProblemId = 1, InputArgs = "[3,3], 6",       ExpectedOutput = "[0,1]", IsSample = false, OrderIndex = 3, Points = 40 },
            new TestCase { Id = 4,  ProblemId = 2, InputArgs = "\"abcabcbb\"",   ExpectedOutput = "3",     IsSample = true,  OrderIndex = 1, Points = 33 },
            new TestCase { Id = 5,  ProblemId = 2, InputArgs = "\"bbbbb\"",      ExpectedOutput = "1",     IsSample = true,  OrderIndex = 2, Points = 33 },
            new TestCase { Id = 6,  ProblemId = 2, InputArgs = "\"pwwkew\"",     ExpectedOutput = "3",     IsSample = false, OrderIndex = 3, Points = 34 },
            new TestCase { Id = 7,  ProblemId = 3, InputArgs = "[1,3], [2]",     ExpectedOutput = "2.0",   IsSample = true,  OrderIndex = 1, Points = 25 },
            new TestCase { Id = 8,  ProblemId = 3, InputArgs = "[1,2], [3,4]",   ExpectedOutput = "2.5",   IsSample = true,  OrderIndex = 2, Points = 25 },
            new TestCase { Id = 9,  ProblemId = 3, InputArgs = "[0,0], [0,0]",   ExpectedOutput = "0.0",   IsSample = false, OrderIndex = 3, Points = 25 },
            new TestCase { Id = 10, ProblemId = 3, InputArgs = "[], [1]",        ExpectedOutput = "1.0",   IsSample = false, OrderIndex = 4, Points = 25 },
            new TestCase { Id = 11, ProblemId = 4, InputArgs = "2",              ExpectedOutput = "2",     IsSample = true,  OrderIndex = 1, Points = 50 },
            new TestCase { Id = 12, ProblemId = 4, InputArgs = "3",              ExpectedOutput = "3",     IsSample = true,  OrderIndex = 2, Points = 50 },
            new TestCase { Id = 13, ProblemId = 5, InputArgs = "2, [[1,0]]",     ExpectedOutput = "true",  IsSample = true,  OrderIndex = 1, Points = 50 },
            new TestCase { Id = 14, ProblemId = 5, InputArgs = "2, [[1,0],[0,1]]", ExpectedOutput = "false", IsSample = true, OrderIndex = 2, Points = 50 }
        );

        // Organizations
        modelBuilder.Entity<Organization>().HasData(
            new Organization { Id = 1, Name = "FER Algorithms",   Description = "Faculty of Electrical Engineering and Computing — Algorithms course. Weekly problem sets and graded contests." },
            new Organization { Id = 2, Name = "Open Source Club", Description = "Student-run open source club. Practice problems, hackathons, and community challenges." }
        );

        // Organization <-> User (join table seed)
        modelBuilder.Entity("OrganizationUser").HasData(
            new { MembersId = 1, OrganizationsId = 1 },
            new { MembersId = 2, OrganizationsId = 1 },
            new { MembersId = 3, OrganizationsId = 1 },
            new { MembersId = 1, OrganizationsId = 2 },
            new { MembersId = 4, OrganizationsId = 2 }
        );

        // ProblemSets
        modelBuilder.Entity<ProblemSet>().HasData(
            new ProblemSet { Id = 1, OrganizationId = 1, Title = "Week 1 — Fundamentals",  Description = "Array manipulation and hashing basics.",                                                         CreatedAt = new DateTime(2025, 10, 1),  IsPublic = true,  OrderIndex = 1 },
            new ProblemSet { Id = 2, OrganizationId = 1, Title = "Advanced Algorithms",     Description = "Dynamic programming, graph traversal, and advanced search techniques.",                          CreatedAt = new DateTime(2025, 11, 1),  IsPublic = false, OrderIndex = 2 },
            new ProblemSet { Id = 3, OrganizationId = 2, Title = "OSC Sprint #1",           Description = "Quick 2-hour sprint with easy and medium problems. Open to all club members.",                   CreatedAt = new DateTime(2025, 12, 1),  IsPublic = true,  OrderIndex = 1 }
        );

        // ProblemSet <-> Problem (join table seed)
        modelBuilder.Entity("ProblemProblemSet").HasData(
            new { ProblemSetId = 1, ProblemsId = 1 },
            new { ProblemSetId = 1, ProblemsId = 2 },
            new { ProblemSetId = 1, ProblemsId = 3 },
            new { ProblemSetId = 2, ProblemsId = 4 },
            new { ProblemSetId = 2, ProblemsId = 5 },
            new { ProblemSetId = 3, ProblemsId = 1 },
            new { ProblemSetId = 3, ProblemsId = 4 }
        );

        // ExecutionResults
        modelBuilder.Entity<ExecutionResult>().HasData(
            new ExecutionResult { Id = 1, Stdout = "[0,1]", Stderr = "", ExitCode = 0, TimedOut = false, MemoryExceeded = false },
            new ExecutionResult { Id = 2, Stdout = "3",     Stderr = "", ExitCode = 0, TimedOut = false, MemoryExceeded = false },
            new ExecutionResult { Id = 3, Stdout = "2.0",   Stderr = "", ExitCode = 0, TimedOut = false, MemoryExceeded = false },
            new ExecutionResult { Id = 4, Stdout = "[0,1]", Stderr = "", ExitCode = 0, TimedOut = false, MemoryExceeded = false },
            new ExecutionResult { Id = 5, Stdout = "",      Stderr = "", ExitCode = 0, TimedOut = true,  MemoryExceeded = false },
            new ExecutionResult { Id = 6, Stdout = "2.5",   Stderr = "", ExitCode = 0, TimedOut = false, MemoryExceeded = false },
            new ExecutionResult { Id = 7, Stdout = "[0,1]", Stderr = "", ExitCode = 0, TimedOut = false, MemoryExceeded = false },
            new ExecutionResult { Id = 8, Stdout = "",      Stderr = "", ExitCode = 1, TimedOut = false, MemoryExceeded = false }
        );

        // Submissions
        modelBuilder.Entity<Submission>().HasData(
            new Submission { Id = 1, UserId = 1, ProblemId = 1, ExecutionResultId = 1, SourceCode = "// correct two sum", Language = "cpp", Status = SubmissionStatus.Accepted,           Score = 100, WallTimeMs =  45, PeakMemoryKb = 2048, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-5)  },
            new Submission { Id = 2, UserId = 1, ProblemId = 2, ExecutionResultId = 2, SourceCode = "// sliding window",  Language = "cpp", Status = SubmissionStatus.Accepted,           Score = 100, WallTimeMs = 120, PeakMemoryKb = 4096, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-10) },
            new Submission { Id = 3, UserId = 1, ProblemId = 3, ExecutionResultId = 3, SourceCode = "// binary search attempt", Language = "cpp", Status = SubmissionStatus.WrongAnswer,  Score =  60, WallTimeMs = 200, PeakMemoryKb = 8192, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-15) },
            new Submission { Id = 4, UserId = 2, ProblemId = 1, ExecutionResultId = 4, SourceCode = "// brute force O(n^2)", Language = "cpp", Status = SubmissionStatus.Accepted,        Score = 100, WallTimeMs = 980, PeakMemoryKb = 2048, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-20) },
            new Submission { Id = 5, UserId = 2, ProblemId = 2, ExecutionResultId = 5, SourceCode = "// naive TLE",        Language = "cpp", Status = SubmissionStatus.TimeLimitExceeded, Score =   0, WallTimeMs = 2001, PeakMemoryKb = 4096, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-25) },
            new Submission { Id = 6, UserId = 2, ProblemId = 3, ExecutionResultId = 6, SourceCode = "// correct binary search", Language = "cpp", Status = SubmissionStatus.Accepted,     Score = 100, WallTimeMs =  55, PeakMemoryKb = 4096, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-30) },
            new Submission { Id = 7, UserId = 3, ProblemId = 1, ExecutionResultId = 7, SourceCode = "// optimal hash map", Language = "cpp", Status = SubmissionStatus.Accepted,          Score = 100, WallTimeMs =  32, PeakMemoryKb = 1024, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-35) },
            new Submission { Id = 8, UserId = 3, ProblemId = 3, ExecutionResultId = 8, SourceCode = "// compile error",    Language = "cpp", Status = SubmissionStatus.CompileError,      Score =   0, WallTimeMs =   0, PeakMemoryKb =    0, SubmittedAt = new DateTime(2026, 5, 7, 0, 0, 0).AddMinutes(-40) }
        );
    }
}
