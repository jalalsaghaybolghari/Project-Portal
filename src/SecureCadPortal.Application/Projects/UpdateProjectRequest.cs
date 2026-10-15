using System.ComponentModel.DataAnnotations;
namespace SecureCadPortal.Application.Projects;

public sealed class UpdateProjectRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Active|Archived)$")]
    public string Status { get; set; } = "Active";
}