using System.ComponentModel.DataAnnotations;
using web.Models;

namespace web.tests.Models;

public class RegistrationModelTests
{
    private Registration CreateValidRegistration() => new()
    {
        EventId = 1,
        Name = "Jane Smith",
        Email = "jane@example.com"
    };

    private List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidRegistration_PassesValidation()
    {
        var reg = CreateValidRegistration();
        var results = ValidateModel(reg);
        Assert.Empty(results);
    }

    [Fact]
    public void MissingName_FailsValidation()
    {
        var reg = CreateValidRegistration();
        reg.Name = null!;
        var results = ValidateModel(reg);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void NameOver100Chars_FailsValidation()
    {
        var reg = CreateValidRegistration();
        reg.Name = new string('x', 101);
        var results = ValidateModel(reg);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void MissingEmail_FailsValidation()
    {
        var reg = CreateValidRegistration();
        reg.Email = null!;
        var results = ValidateModel(reg);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void InvalidEmailFormat_FailsValidation()
    {
        var reg = CreateValidRegistration();
        reg.Email = "not-an-email";
        var results = ValidateModel(reg);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void MissingEventId_FailsValidation()
    {
        var reg = CreateValidRegistration();
        reg.EventId = 0;
        var results = ValidateModel(reg);
        Assert.Contains(results, r => r.MemberNames.Contains("EventId"));
    }
}
