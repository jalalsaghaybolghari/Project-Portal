using System.ComponentModel.DataAnnotations;

namespace SecureCadPortal.Application.Projects;

public class CreateProjectRequest
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