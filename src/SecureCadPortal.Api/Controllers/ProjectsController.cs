
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureCadPortal.Application.Projects;
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Project>> GetProjectById(int id)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(
        [FromBody] CreateProjectRequest request)
    {
        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProjectById),
            new { id = project.Id },
            project);
    }
}
