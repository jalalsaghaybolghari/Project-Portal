using SecureCadPortal.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace SecureCadPortal.Application.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IProjectRepository repository, ILogger<ProjectService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<List<Project>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Project?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public async Task<Project> CreateAsync(
        CreateProjectRequest request)
    {
        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(project);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Project {ProjectId} created successfully",project.Id);

        return project;
    }

    public async Task<bool> UpdateAsync(
    int id,
    UpdateProjectRequest request)
    {
        var project = await _repository.GetByIdAsync(id);

        if (project is null)
        {
            return false;
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description.Trim();
        project.Status = request.Status;

        var isUpdated = await _repository.UpdateAsync(project);
        _logger.LogInformation("Project {ProjectId} updated successfully",project.Id);
        return isUpdated;
    }

    public Task<bool> DeleteAsync(int id)
    {
        var isDeleted = _repository.DeleteAsync(id);
        _logger.LogInformation("Project {ProjectId} deleted successfully",id);
        return isDeleted;
    }
}