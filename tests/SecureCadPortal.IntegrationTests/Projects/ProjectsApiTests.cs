
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureCadPortal.Domain.Entities;
using SecureCadPortal.Infrastructure.Data;
using SecureCadPortal.IntegrationTests.Authentication;

namespace SecureCadPortal.IntegrationTests.Projects;

public class ProjectsApiTests
{
    private const string ProjectsEndpoint = "/api/projects";

    [Fact]
    public async Task PostProject_ThenGetProjects_ReturnsSavedProject()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = CreateAuthenticatedClient(factory);

        var request = new
        {
            name = "CAD Integration Project",
            description = "Engineering document management pilot",
            status = "Active"
        };

        var postResponse = await client.PostAsJsonAsync(
            ProjectsEndpoint,
            request);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var projects = await client.GetFromJsonAsync<List<ProjectResponse>>(
            ProjectsEndpoint);

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
    public async Task GetProjectById_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync(
            $"{ProjectsEndpoint}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostProject_WithShortName_ReturnsBadRequest()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = CreateAuthenticatedClient(factory);

        var request = new
        {
            name = "AB",
            description = "Invalid test",
            status = "Active"
        };

        var response = await client.PostAsJsonAsync(
            ProjectsEndpoint,
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_WhenProjectExists_ReturnsNoContent()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = CreateAuthenticatedClient(factory);

        var createResponse = await client.PostAsJsonAsync(
            ProjectsEndpoint,
            new
            {
                name = "Project to Delete",
                description = "Integration test project",
                status = "Active"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdProject = await createResponse.Content
            .ReadFromJsonAsync<Project>();

        Assert.NotNull(createdProject);

        var deleteResponse = await client.DeleteAsync(
            $"{ProjectsEndpoint}/{createdProject.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"{ProjectsEndpoint}/{createdProject.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.DeleteAsync(
            $"{ProjectsEndpoint}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_WithoutAuthentication_ReturnsUnauthorized()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ProjectsEndpoint);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_WithTestAuthentication_ReturnsSuccess()
    {
        await using var factory = new ProjectsApiFactory();
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync(ProjectsEndpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static HttpClient CreateAuthenticatedClient(
        ProjectsApiFactory factory)
    {
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.HeaderName,
            "true");

        return client;
    }

    private sealed class ProjectsApiFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            var databaseName = $"ProjectsApiTests_{Guid.NewGuid():N}";

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<
                    IDbContextOptionsConfiguration<AppDbContext>>();

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme =
                            TestAuthHandler.SchemeName;

                        options.DefaultChallengeScheme =
                            TestAuthHandler.SchemeName;
                    })
                    .AddScheme<
                        AuthenticationSchemeOptions,
                        TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        _ => { });
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
