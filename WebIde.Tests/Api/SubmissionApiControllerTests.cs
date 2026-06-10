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

public class SubmissionApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SubmissionApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<(User user, Problem problem, Submission sub)> SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();

        var user = new User
        {
            Username     = "sub-user",
            Email        = "sub@example.com",
            DisplayName  = "Sub User",
            Role         = UserRole.Student,
            RegisteredAt = DateTime.UtcNow,
        };
        db.DomainUsers.Add(user);

        var problem = new Problem
        {
            Title          = "Sub Problem",
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
            Language    = "python",
            SourceCode  = "print('hello')",
            Status      = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
        };
        db.Submissions.Add(sub);
        await db.SaveChangesAsync();
        return (user, problem, sub);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/submission");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<SubmissionDto>>();
        Assert.NotNull(list);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var (_, _, sub) = await SeedAsync();
        var response = await _client.GetAsync($"/api/submission/{sub.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<SubmissionDto>();
        Assert.Equal(sub.Id, dto!.Id);
        Assert.Equal(sub.Language, dto.Language);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _client.GetAsync("/api/submission/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        var (user, problem, _) = await SeedAsync();
        var dto = new CreateSubmissionDto
        {
            ProblemId  = problem.Id,
            Language   = "python",
            SourceCode = "print('hi')",
        };
        var response = await _client.PostAsJsonAsync("/api/submission", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingSourceCode_Returns400()
    {
        var dto = new CreateSubmissionDto { ProblemId = 1, Language = "python", SourceCode = "" };
        var response = await _client.PostAsJsonAsync("/api/submission", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
