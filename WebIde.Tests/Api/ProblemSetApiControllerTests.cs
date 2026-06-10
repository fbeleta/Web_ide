using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Tests.Infrastructure;
using WebIde.Web.DTOs;
using Xunit;

namespace WebIde.Tests.Api;

public class ProblemSetApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProblemSetApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<ProblemSet> SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();

        var org = new Organization { Name = "Test Org", Description = "" };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var ps = new ProblemSet
        {
            Title          = "Set A",
            Description    = "desc",
            IsPublic       = true,
            OrganizationId = org.Id,
            CreatedAt      = DateTime.UtcNow,
        };
        db.ProblemSets.Add(ps);
        await db.SaveChangesAsync();
        return ps;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/problemset");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var ps = await SeedAsync();
        var response = await _client.GetAsync($"/api/problemset/{ps.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProblemSetDto>();
        Assert.Equal(ps.Title, dto!.Title);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _client.GetAsync("/api/problemset/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        var ps = await SeedAsync();
        var dto = new CreateProblemSetDto
        {
            Title          = "New Set",
            Description    = "desc",
            IsPublic       = true,
            OrganizationId = ps.OrganizationId,
        };
        var response = await _client.PostAsJsonAsync("/api/problemset", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyTitle_Returns400()
    {
        var dto = new CreateProblemSetDto { Title = "" };
        var response = await _client.PostAsJsonAsync("/api/problemset", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        var ps = await SeedAsync();
        var dto = new UpdateProblemSetDto { Title = "Updated Set" };
        var response = await _client.PutAsJsonAsync($"/api/problemset/{ps.Id}", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExisting_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/problemset/99999", new UpdateProblemSetDto { Title = "x" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        var ps = await SeedAsync();
        var response = await _client.DeleteAsync($"/api/problemset/{ps.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExisting_Returns404()
    {
        var response = await _client.DeleteAsync("/api/problemset/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
