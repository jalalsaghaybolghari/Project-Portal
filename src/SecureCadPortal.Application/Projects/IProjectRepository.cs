using SecureCadPortal.Domain.Entities;

namespace SecureCadPortal.Application.Projects;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync();

    Task<Project?> GetByIdAsync(int id);

    Task AddAsync(Project project);

    Task SaveChangesAsync();
}