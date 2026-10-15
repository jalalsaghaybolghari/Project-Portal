using SecureCadPortal.Application.Projects;
using SecureCadPortal.Domain.Entities;
using Xunit;

namespace SecureCadPortal.UnitTests.Projects;

public class ProjectServiceTests
{
    [Fact]
    public async Task CreateAsync_TrimsInputAndSavesProject()
    {
        // Arrange
        var repository = new FakeProjectRepository();
        var service = new ProjectService(repository);

        var request = new CreateProjectRequest
        {
            Name = "  CAD Portal  ",
            Description = "  Manage CAD projects  ",
            Status = "Active"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.Equal("CAD Portal", result.Name);
        Assert.Equal("Manage CAD projects", result.Description);
        Assert.Equal("Active", result.Status);
        Assert.Equal(DateTimeKind.Utc, result.CreatedAtUtc.Kind);
        Assert.Same(result, repository.AddedProject);
        Assert.True(repository.SaveChangesCalled);
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        public Project? AddedProject { get; private set; }

        public bool SaveChangesCalled { get; private set; }

        public Task<List<Project>> GetAllAsync()
        {
            return Task.FromResult(new List<Project>());
        }

        public Task<Project?> GetByIdAsync(int id)
        {
            return Task.FromResult<Project?>(null);
        }

        public Task AddAsync(Project project)
        {
            AddedProject = project;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }
}