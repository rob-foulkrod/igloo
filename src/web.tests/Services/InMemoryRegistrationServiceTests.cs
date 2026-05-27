using web.Models;
using web.Services;

namespace web.tests.Services;

public class InMemoryRegistrationServiceTests
{
    private (InMemoryEventService eventService, InMemoryRegistrationService regService) CreateServices()
    {
        var eventService = new InMemoryEventService();
        var regService = new InMemoryRegistrationService(eventService);
        return (eventService, regService);
    }

    [Fact]
    public void Create_ValidRegistration_AssignsIdAndTimestamp()
    {
        var (_, regService) = CreateServices();
        var reg = new Registration { EventId = 1, Name = "Jane", Email = "jane@test.com" };

        var created = regService.Create(reg);

        Assert.Equal(1, created.Id);
        Assert.True(created.RegisteredAt <= DateTime.UtcNow);
        Assert.True(created.RegisteredAt > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void Create_AtCapacity_ThrowsInvalidOperation()
    {
        var (eventService, regService) = CreateServices();
        // Hot Cocoa Social has capacity 30
        var evt = eventService.GetAll().First(e => e.Title == "Hot Cocoa Social");

        for (int i = 0; i < evt.Capacity; i++)
        {
            regService.Create(new Registration
            {
                EventId = evt.Id,
                Name = $"Person {i}",
                Email = $"person{i}@test.com"
            });
        }

        Assert.Throws<InvalidOperationException>(() =>
            regService.Create(new Registration
            {
                EventId = evt.Id,
                Name = "One Too Many",
                Email = "extra@test.com"
            }));
    }

    [Fact]
    public void Create_UnknownEventId_ThrowsKeyNotFoundException()
    {
        var (_, regService) = CreateServices();

        Assert.Throws<KeyNotFoundException>(() =>
            regService.Create(new Registration
            {
                EventId = 999,
                Name = "Ghost",
                Email = "ghost@test.com"
            }));
    }

    [Fact]
    public void GetByEventId_ReturnsOnlyMatchingRegistrations()
    {
        var (_, regService) = CreateServices();
        regService.Create(new Registration { EventId = 1, Name = "A", Email = "a@test.com" });
        regService.Create(new Registration { EventId = 2, Name = "B", Email = "b@test.com" });
        regService.Create(new Registration { EventId = 1, Name = "C", Email = "c@test.com" });

        var result = regService.GetByEventId(1).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(1, r.EventId));
    }

    [Fact]
    public void GetById_ExistingId_ReturnsRegistration()
    {
        var (_, regService) = CreateServices();
        regService.Create(new Registration { EventId = 1, Name = "A", Email = "a@test.com" });

        var reg = regService.GetById(1);
        Assert.NotNull(reg);
        Assert.Equal("A", reg.Name);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var (_, regService) = CreateServices();
        Assert.Null(regService.GetById(999));
    }

    [Fact]
    public void Delete_ExistingId_RemovesAndReturnsTrue()
    {
        var (_, regService) = CreateServices();
        regService.Create(new Registration { EventId = 1, Name = "A", Email = "a@test.com" });

        Assert.True(regService.Delete(1));
        Assert.Null(regService.GetById(1));
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsFalse()
    {
        var (_, regService) = CreateServices();
        Assert.False(regService.Delete(999));
    }

    [Fact]
    public void GetRegistrationCount_ReturnsCorrectCount()
    {
        var (_, regService) = CreateServices();
        regService.Create(new Registration { EventId = 1, Name = "A", Email = "a@test.com" });
        regService.Create(new Registration { EventId = 1, Name = "B", Email = "b@test.com" });

        Assert.Equal(2, regService.GetRegistrationCount(1));
        Assert.Equal(0, regService.GetRegistrationCount(2));
    }

    [Fact]
    public void HasCapacity_EventWithSpace_ReturnsTrue()
    {
        var (_, regService) = CreateServices();
        Assert.True(regService.HasCapacity(1));
    }

    [Fact]
    public void HasCapacity_NonExistingEvent_ReturnsFalse()
    {
        var (_, regService) = CreateServices();
        Assert.False(regService.HasCapacity(999));
    }
}
