
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureCadPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace SecureCadPortal.IntegrationTests.Projects;

public class ProjectsApiTests
{
    [Fact]
    public async Task PostProject_ThenGetProjects_ReturnsSavedProject()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            name = "CAD Integration Project",
            description = "Engineering document management pilot",
            status = "Active"
        };

        var postResponse = await client.PostAsJsonAsync(
            "/api/projects", request);

        var responseBody = await postResponse.Content.ReadAsStringAsync();

        Console.WriteLine($"POST status: {(int)postResponse.StatusCode}");
        Console.WriteLine($"POST response: {responseBody}");

        Assert.True(
            postResponse.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created, got {(int)postResponse.StatusCode} " +
            $"{postResponse.StatusCode}. Response: {responseBody}");

        var projects = await client.GetFromJsonAsync<List<ProjectResponse>>(
            "/api/projects");

        Assert.NotNull(projects);

        var project = Assert.Single(
            projects,
            p => p.Name == "CAD Integration Project");

        Assert.Equal("Active", project.Status);
        Assert.Equal(
            "Engineering document management pilot",
            project.Description);
    }

    [Fact]
    public async Task PostProject_WithShortName_ReturnsBadRequest()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            name = "AB",
            description = "Invalid test",
            status = "Active"
        };

        var response = await client.PostAsJsonAsync(
            "/api/projects", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed class ProjectsApiFactory
        : WebApplicationFactory<Program>
    {
        
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
        services.RemoveAll<AppDbContext>();
        services.RemoveAll<DbContextOptions<AppDbContext>>();
        services.RemoveAll<
            IDbContextOptionsConfiguration<AppDbContext>>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("ProjectsApiTests"));
    });
}

    }

    private sealed record ProjectResponse(
        int Id,
        string Name,
        string Description,
        string Status,
        DateTime CreatedAtUtc);
}
