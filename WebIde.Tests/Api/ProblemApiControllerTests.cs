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

public class ProblemApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProblemApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<Problem> SeedProblemAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();
        var p = new Problem
        {
            Title          = "Test Problem",
            Description    = "A test problem",
            Difficulty     = DifficultyLevel.Easy,
            TimeLimitMs    = 2000,
            MemoryLimitKb  = 262144,
            AuthorUsername = "testuser",
            CreatedAt      = DateTime.UtcNow,
        };
        db.Problems.Add(p);
        await db.SaveChangesAsync();
        return p;
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        await SeedProblemAsync();
        var response = await _client.GetAsync("/api/problem");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<ProblemDto>>();
        Assert.NotNull(list);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var problem = await SeedProblemAsync();
        var response = await _client.GetAsync($"/api/problem/{problem.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProblemDto>();
        Assert.Equal(problem.Id, dto!.Id);
        Assert.Equal(problem.Title, dto.Title);
    }

    [Fact]
    public async Task GetById_NonExistingId_Returns404()
    {
        var response = await _client.GetAsync("/api/problem/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_ReturnsFilteredResults()
    {
        await SeedProblemAsync();
        var response = await _client.GetAsync("/api/problem/search?q=Test");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<ProblemDto>>();
        Assert.NotNull(list);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        var dto = new CreateProblemDto
        {
            Title          = "New Problem",
            Description    = "Description",
            Difficulty     = "Easy",
            TimeLimitMs    = 2000,
            MemoryLimitKb  = 262144,
            AuthorUsername = "admin",
        };
        var response = await _client.PostAsJsonAsync("/api/problem", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProblemDto>();
        Assert.Equal("New Problem", created!.Title);
    }

    [Fact]
    public async Task Create_MissingTitle_Returns400()
    {
        var dto = new CreateProblemDto { Title = "", Description = "x" };
        var response = await _client.PostAsJsonAsync("/api/problem", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        var problem = await SeedProblemAsync();
        var dto = new UpdateProblemDto { Title = "Updated Title" };
        var response = await _client.PutAsJsonAsync($"/api/problem/{problem.Id}", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProblemDto>();
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task Update_NonExistingId_Returns404()
    {
        var dto = new UpdateProblemDto { Title = "x" };
        var response = await _client.PutAsJsonAsync("/api/problem/99999", dto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        var problem = await SeedProblemAsync();
        var response = await _client.DeleteAsync($"/api/problem/{problem.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingId_Returns404()
    {
        var response = await _client.DeleteAsync("/api/problem/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
