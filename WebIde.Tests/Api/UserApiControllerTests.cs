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

public class UserApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UserApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<User> SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();
        var u = new User
        {
            Username    = "testuser",
            Email       = "testuser@example.com",
            DisplayName = "Test User",
            Role        = UserRole.Student,
            RegisteredAt = DateTime.UtcNow,
        };
        db.DomainUsers.Add(u);
        await db.SaveChangesAsync();
        return u;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/user");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var user = await SeedUserAsync();
        var response = await _client.GetAsync($"/api/user/{user.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(user.Username, dto!.Username);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _client.GetAsync("/api/user/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        var dto = new CreateUserDto
        {
            Username    = "newuser",
            Email       = "newuser@example.com",
            DisplayName = "New User",
        };
        var response = await _client.PostAsJsonAsync("/api/user", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyUsername_Returns400()
    {
        var dto = new CreateUserDto { Username = "", Email = "x@x.com", DisplayName = "x" };
        var response = await _client.PostAsJsonAsync("/api/user", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        var user = await SeedUserAsync();
        var dto = new UpdateUserDto { DisplayName = "Updated Name" };
        var response = await _client.PutAsJsonAsync($"/api/user/{user.Id}", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExisting_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/user/99999", new UpdateUserDto { DisplayName = "x" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        var user = await SeedUserAsync();
        var response = await _client.DeleteAsync($"/api/user/{user.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExisting_Returns404()
    {
        var response = await _client.DeleteAsync("/api/user/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
