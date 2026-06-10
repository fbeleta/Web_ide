using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Model.Enums;
using WebIde.Tests.Infrastructure;
using WebIde.Web.DTOs;
using Xunit;

namespace WebIde.Tests.Api;

public class ExecutionResultApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ExecutionResultApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<ExecutionResult> SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();

        var user = new User
        {
            Username     = "er-user",
            Email        = "er@example.com",
            DisplayName  = "ER User",
            Role         = UserRole.Student,
            RegisteredAt = DateTime.UtcNow,
        };
        db.DomainUsers.Add(user);

        var problem = new Problem
        {
            Title          = "ER Problem",
            Description    = "desc",
            Difficulty     = DifficultyLevel.Easy,
            TimeLimitMs    = 2000,
            MemoryLimitKb  = 262144,
            AuthorUsername = "admin",
            CreatedAt      = DateTime.UtcNow,
        };
        db.Problems.Add(problem);
        await db.SaveChangesAsync();

        var sub = new Submission
        {
            UserId      = user.Id,
            ProblemId   = problem.Id,
            Language    = "python3",
            SourceCode  = "print(2)",
            Status      = SubmissionStatus.Accepted,
            SubmittedAt = DateTime.UtcNow,
        };
        db.Submissions.Add(sub);
        await db.SaveChangesAsync();

        var tc = new TestCase
        {
            ProblemId      = problem.Id,
            InputArgs      = "1",
            ExpectedOutput = "2",
            IsSample       = true,
            OrderIndex     = 1,
            Points         = 10,
        };
        db.TestCases.Add(tc);
        await db.SaveChangesAsync();

        var er = new ExecutionResult
        {
            SubmissionId   = sub.Id,
            TestCaseId     = tc.Id,
            Stdout         = "2",
            Stderr         = "",
            ExitCode       = 0,
            WallTimeMs     = 100,
            PeakMemoryKb   = 1024,
            Verdict        = Verdict.Accepted,
            TimedOut       = false,
            MemoryExceeded = false,
        };
        db.ExecutionResults.Add(er);
        await db.SaveChangesAsync();
        return er;
    }

    [Fact]
    public async Task GetAll_Admin_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/executionresult");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var er = await SeedAsync();
        var response = await _client.GetAsync($"/api/executionresult/{er.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ExecutionResultDto>();
        Assert.Equal(er.Id, dto!.Id);
        Assert.Equal(er.Stdout, dto.Stdout);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _client.GetAsync("/api/executionresult/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
