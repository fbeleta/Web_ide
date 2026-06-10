using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Tests.Infrastructure;
using WebIde.Web.DTOs;
using Xunit;

namespace WebIde.Tests.Api;

public class TagApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TagApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<Tag> SeedTagAsync(string name = "algorithms")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();
        var t = new Tag { Name = name };
        db.Tags.Add(t);
        await db.SaveChangesAsync();
        return t;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/tag");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var tag = await SeedTagAsync("graphs");
        var response = await _client.GetAsync($"/api/tag/{tag.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal(tag.Name, dto!.Name);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _client.GetAsync("/api/tag/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        var dto = new CreateTagDto { Name = "dynamic-programming" };
        var response = await _client.PostAsJsonAsync("/api/tag", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var dto = new CreateTagDto { Name = "" };
        var response = await _client.PostAsJsonAsync("/api/tag", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        var tag = await SeedTagAsync("old-name");
        var dto = new UpdateTagDto { Name = "new-name" };
        var response = await _client.PutAsJsonAsync($"/api/tag/{tag.Id}", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExisting_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/tag/99999", new UpdateTagDto { Name = "x" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        var tag = await SeedTagAsync("to-delete");
        var response = await _client.DeleteAsync($"/api/tag/{tag.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExisting_Returns404()
    {
        var response = await _client.DeleteAsync("/api/tag/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
