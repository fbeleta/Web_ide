using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Tests.Infrastructure;
using WebIde.Web.DTOs;
using Xunit;

namespace WebIde.Tests.Api;

public class OrganizationApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public OrganizationApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private async Task<Organization> SeedOrgAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebIdeDbContext>();
        var o = new Organization { Name = "Test Org", Description = "A test org" };
        db.Organizations.Add(o);
        await db.SaveChangesAsync();
        return o;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/organization");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var org = await SeedOrgAsync();
        var response = await _client.GetAsync($"/api/organization/{org.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrganizationDto>();
        Assert.Equal(org.Name, dto!.Name);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _client.GetAsync("/api/organization/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        var dto = new CreateOrganizationDto { Name = "New Org", Description = "desc" };
        var response = await _client.PostAsJsonAsync("/api/organization", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var dto = new CreateOrganizationDto { Name = "" };
        var response = await _client.PostAsJsonAsync("/api/organization", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        var org = await SeedOrgAsync();
        var dto = new UpdateOrganizationDto { Name = "Updated Org" };
        var response = await _client.PutAsJsonAsync($"/api/organization/{org.Id}", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExisting_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/organization/99999", new UpdateOrganizationDto { Name = "x" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        var org = await SeedOrgAsync();
        var response = await _client.DeleteAsync($"/api/organization/{org.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExisting_Returns404()
    {
        var response = await _client.DeleteAsync("/api/organization/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
