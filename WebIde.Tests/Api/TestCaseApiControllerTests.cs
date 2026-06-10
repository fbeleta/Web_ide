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

public class TestCaseApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TestCaseApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<(Problem problem, TestCase tc)> SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();
        var p = new Problem
        {
            Title          = "TC Problem",
            Description    = "desc",
            Difficulty     = DifficultyLevel.Easy,
            TimeLimitMs    = 2000,
            MemoryLimitKb  = 262144,
            AuthorUsername = "admin",
            CreatedAt      = DateTime.UtcNow,
        };
        db.Problems.Add(p);
        await db.SaveChangesAsync();

        var tc = new TestCase
        {
            ProblemId      = p.Id,
            InputArgs      = "1 2",
            ExpectedOutput = "3",
            IsSample       = true,
            OrderIndex     = 1,
            Points         = 10,
        };
        db.TestCases.Add(tc);
        await db.SaveChangesAsync();
        return (p, tc);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/testcase");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var (_, tc) = await SeedAsync();
        var response = await _client.GetAsync($"/api/testcase/{tc.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TestCaseDto>();
        Assert.Equal(tc.InputArgs, dto!.InputArgs);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _client.GetAsync("/api/testcase/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        var (p, _) = await SeedAsync();
        var dto = new CreateTestCaseDto
        {
            ProblemId      = p.Id,
            InputArgs      = "5 6",
            ExpectedOutput = "11",
            IsSample       = false,
            Points         = 5,
        };
        var response = await _client.PostAsJsonAsync("/api/testcase", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingFields_Returns400()
    {
        var dto = new CreateTestCaseDto { ProblemId = 1, InputArgs = "", ExpectedOutput = "" };
        var response = await _client.PostAsJsonAsync("/api/testcase", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        var (_, tc) = await SeedAsync();
        var dto = new UpdateTestCaseDto { ExpectedOutput = "99" };
        var response = await _client.PutAsJsonAsync($"/api/testcase/{tc.Id}", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExisting_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/testcase/99999", new UpdateTestCaseDto { Points = 1 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        var (_, tc) = await SeedAsync();
        var response = await _client.DeleteAsync($"/api/testcase/{tc.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExisting_Returns404()
    {
        var response = await _client.DeleteAsync("/api/testcase/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
