using web.Models;
using web.Services;

namespace web.tests.Services;

public class InMemoryEventServiceTests
{
    private InMemoryEventService CreateService() => new();

    [Fact]
    public void GetAll_ReturnsSeededEvents()
    {
        var service = CreateService();
        var events = service.GetAll().ToList();
        Assert.Equal(3, events.Count);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsEvent()
    {
        var service = CreateService();
        var evt = service.GetById(1);
        Assert.NotNull(evt);
        Assert.Equal("Community Ice Skating", evt.Title);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var service = CreateService();
        var evt = service.GetById(999);
        Assert.Null(evt);
    }

    [Fact]
    public void Create_AssignsIdAndAddsEvent()
    {
        var service = CreateService();
        var newEvent = new Event
        {
            Title = "New Event",
            Description = "Description",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(10).AddHours(2),
            Location = "Park",
            Capacity = 20
        };

        var created = service.Create(newEvent);

        Assert.True(created.Id > 0);
        Assert.Contains(created, service.GetAll());
        Assert.Equal(4, service.GetAll().Count());
    }

    [Fact]
    public void Update_ExistingEvent_UpdatesFields()
    {
        var service = CreateService();
        var evt = service.GetById(1)!;
        evt.Title = "Updated Title";

        var updated = service.Update(evt);

        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Title", service.GetById(1)!.Title);
    }

    [Fact]
    public void Update_NonExistingEvent_ThrowsKeyNotFoundException()
    {
        var service = CreateService();
        var evt = new Event { Id = 999, Title = "X", Description = "X", Location = "X", Capacity = 1 };

        Assert.Throws<KeyNotFoundException>(() => service.Update(evt));
    }

    [Fact]
    public void Delete_ExistingId_RemovesAndReturnsTrue()
    {
        var service = CreateService();
        var result = service.Delete(1);

        Assert.True(result);
        Assert.Null(service.GetById(1));
        Assert.Equal(2, service.GetAll().Count());
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsFalse()
    {
        var service = CreateService();
        var result = service.Delete(999);
        Assert.False(result);
    }
}
