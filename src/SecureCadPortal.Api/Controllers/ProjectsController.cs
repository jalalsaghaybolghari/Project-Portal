
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureCadPortal.Domain.Entities;
using SecureCadPortal.Infrastructure.Data;

namespace SecureCadPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProjectsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(projects);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(
        [FromBody] Project project)
    {
        project.Id = 0;
        project.CreatedAtUtc = DateTime.UtcNow;

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            project);
    }
}
