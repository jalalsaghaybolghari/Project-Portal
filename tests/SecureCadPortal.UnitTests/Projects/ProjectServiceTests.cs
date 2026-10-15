
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
        Assert.Contains(result, repository.Projects);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task DeleteAsync_WhenProjectExists_ReturnsTrue()
    {
        // Arrange
        var repository = new FakeProjectRepository();
        var project = CreateProject(1);
        repository.Seed(project);

        // Act
        var result = await repository.DeleteAsync(project.Id);

        // Assert
        Assert.True(result);
        Assert.DoesNotContain(project, repository.Projects);
    }

    [Fact]
    public async Task DeleteAsync_WhenProjectDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new FakeProjectRepository();

        // Act
        var result = await repository.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    private static Project CreateProject(int id)
    {
        return new Project
        {
            Id = id,
            Name = $"Project {id}",
            Description = "Test project",
            Status = "Active",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        private readonly List<Project> _projects = [];

        public IReadOnlyList<Project> Projects => _projects;

        public bool SaveChangesCalled { get; private set; }

        public void Seed(Project project)
        {
            _projects.Add(project);
        }

        public Task<List<Project>> GetAllAsync()
        {
            return Task.FromResult(_projects.ToList());
        }

        public Task<Project?> GetByIdAsync(int id)
        {
            var project = _projects.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(project);
        }

        public Task AddAsync(Project project)
        {
            _projects.Add(project);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Project project)
        {
            var index = _projects.FindIndex(p => p.Id == project.Id);

            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _projects[index] = project;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var project = _projects.FirstOrDefault(p => p.Id == id);

            if (project is null)
            {
                return Task.FromResult(false);
            }

            _projects.Remove(project);
            return Task.FromResult(true);
        }
    }
}
