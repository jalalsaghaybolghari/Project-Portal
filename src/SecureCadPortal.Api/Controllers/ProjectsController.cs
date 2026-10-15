
using Microsoft.AspNetCore.Mvc;
using SecureCadPortal.Application.Projects;
using SecureCadPortal.Domain.Entities;

namespace SecureCadPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await _projectService.GetAllAsync();

        return Ok(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Project>> GetProjectById(int id)
    {
        var project = await _projectService.GetByIdAsync(id);

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
        var project = await _projectService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetProjectById),
            new { id = project.Id },
            project);
    }
}
