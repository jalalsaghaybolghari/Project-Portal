using SecureCadPortal.Domain.Entities;

namespace SecureCadPortal.Application.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;

    public ProjectService(IProjectRepository repository)
    {
        _repository = repository;
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

        return project;
    }
}