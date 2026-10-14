
using System.ComponentModel.DataAnnotations;
using SecureCadPortal.Application.Projects;

namespace SecureCadPortal.UnitTests.Projects;

public class CreateProjectRequestTests
{
    private static IList<ValidationResult> Validate(
        CreateProjectRequest request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        Validator.TryValidateObject(
            request,
            context,
            results,
            validateAllProperties: true);

        return results;
    }

    [Fact]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = new CreateProjectRequest
        {
            Name = "CAD Integration",
            Description = "Engineering project",
            Status = "Active"
        };

        var results = Validate(request);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    public void InvalidName_ShouldFailValidation(string name)
    {
        var request = new CreateProjectRequest
        {
            Name = name,
            Status = "Active"
        };

        var results = Validate(request);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(nameof(request.Name)));
    }

    [Theory]
    [InlineData("Deleted")]
    [InlineData("Pending")]
    public void InvalidStatus_ShouldFailValidation(string status)
    {
        var request = new CreateProjectRequest
        {
            Name = "CAD Integration",
            Status = status
        };

        var results = Validate(request);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(nameof(request.Status)));
    }

    [Fact]
    public void DescriptionOver1000Characters_ShouldFailValidation()
    {
        var request = new CreateProjectRequest
        {
            Name = "CAD Integration",
            Description = new string('A', 1001),
            Status = "Active"
        };

        var results = Validate(request);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(request.Description)));
    }
}
