using SecureCadPortal.Domain.Entities;

namespace SecureCadPortal.Application.Projects;

public interface IProjectService
{
    Task<List<Project>> GetAllAsync();

    Task<Project?> GetByIdAsync(int id);

    Task<Project> CreateAsync(CreateProjectRequest request);

    Task<bool> UpdateAsync(
    int id,
    UpdateProjectRequest request);

    Task<bool> DeleteAsync(int id);
}