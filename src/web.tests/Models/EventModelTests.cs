using System.ComponentModel.DataAnnotations;
using web.Models;

namespace web.tests.Models;

public class EventModelTests
{
    private Event CreateValidEvent() => new()
    {
        Title = "Community Ice Skating",
        Description = "A fun ice skating event for the whole community.",
        StartDate = DateTime.UtcNow.AddDays(7),
        EndDate = DateTime.UtcNow.AddDays(7).AddHours(3),
        Location = "City Rink",
        Capacity = 50
    };

    private List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidEvent_PassesValidation()
    {
        var evt = CreateValidEvent();
        var results = ValidateModel(evt);
        Assert.Empty(results);
    }

    [Fact]
    public void MissingTitle_FailsValidation()
    {
        var evt = CreateValidEvent();
        evt.Title = null!;
        var results = ValidateModel(evt);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TitleOver100Chars_FailsValidation()
    {
        var evt = CreateValidEvent();
        evt.Title = new string('x', 101);
        var results = ValidateModel(evt);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void MissingDescription_FailsValidation()
    {
        var evt = CreateValidEvent();
        evt.Description = null!;
        var results = ValidateModel(evt);
        Assert.Contains(results, r => r.MemberNames.Contains("Description"));
    }

    [Fact]
    public void DescriptionOver500Chars_FailsValidation()
    {
        var evt = CreateValidEvent();
        evt.Description = new string('x', 501);
        var results = ValidateModel(evt);
        Assert.Contains(results, r => r.MemberNames.Contains("Description"));
    }

    [Fact]
    public void MissingLocation_FailsValidation()
    {
        var evt = CreateValidEvent();
        evt.Location = null!;
        var results = ValidateModel(evt);
        Assert.Contains(results, r => r.MemberNames.Contains("Location"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    public void CapacityOutOfRange_FailsValidation(int capacity)
    {
        var evt = CreateValidEvent();
        evt.Capacity = capacity;
        var results = ValidateModel(evt);
        Assert.Contains(results, r => r.MemberNames.Contains("Capacity"));
    }
}
