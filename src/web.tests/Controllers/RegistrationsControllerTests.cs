using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using web.Controllers;
using web.Models;
using web.Services;

namespace web.tests.Controllers;

public class RegistrationsControllerTests
{
    private readonly InMemoryEventService _eventService;
    private readonly InMemoryRegistrationService _registrationService;
    private readonly RegistrationsController _controller;

    public RegistrationsControllerTests()
    {
        _eventService = new InMemoryEventService();
        _registrationService = new InMemoryRegistrationService(_eventService);
        _controller = new RegistrationsController(
            _registrationService,
            _eventService,
            NullLogger<RegistrationsController>.Instance);
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(), new TestTempDataProvider());
    }

    private class TestTempDataProvider : ITempDataProvider
    {
        private Dictionary<string, object?> _data = new();
        public IDictionary<string, object?> LoadTempData(HttpContext context) => _data;
        public void SaveTempData(HttpContext context, IDictionary<string, object?> values) =>
            _data = new Dictionary<string, object?>(values);
    }

    [Fact]
    public void Create_Get_ValidEventId_ReturnsView()
    {
        var events = _eventService.GetAll().ToList();
        var eventId = events.First().Id;

        var result = _controller.Create(eventId) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<Registration>(result.Model);
        Assert.Equal(eventId, model.EventId);
    }

    [Fact]
    public void Create_Get_NonExistingEventId_ReturnsNotFound()
    {
        var result = _controller.Create(9999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_Get_FullCapacityEvent_RedirectsToDetails()
    {
        // Create event with capacity 1
        var evt = _eventService.Create(new Event
        {
            Title = "Tiny Event",
            Description = "Only 1 spot",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(10).AddHours(2),
            Location = "Small Room",
            Capacity = 1
        });

        // Fill it
        _registrationService.Create(new Registration
        {
            EventId = evt.Id,
            Name = "First Person",
            Email = "first@example.com"
        });

        var result = _controller.Create(evt.Id) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Details", result.ActionName);
        Assert.Equal("Events", result.ControllerName);
    }

    [Fact]
    public void Create_Post_ValidRegistration_RedirectsToDetails()
    {
        var events = _eventService.GetAll().ToList();
        var eventId = events.First().Id;

        var registration = new Registration
        {
            EventId = eventId,
            Name = "Test User",
            Email = "test@example.com"
        };

        var result = _controller.Create(registration) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Details", result.ActionName);
        Assert.Equal("Events", result.ControllerName);
    }

    [Fact]
    public void Create_Post_InvalidModel_ReturnsView()
    {
        var events = _eventService.GetAll().ToList();
        var eventId = events.First().Id;

        var registration = new Registration { EventId = eventId };
        _controller.ModelState.AddModelError("Name", "Required");

        var result = _controller.Create(registration) as ViewResult;

        Assert.NotNull(result);
        Assert.Equal(registration, result.Model);
    }

    [Fact]
    public void Create_Post_NonExistingEvent_ReturnsNotFound()
    {
        var registration = new Registration
        {
            EventId = 9999,
            Name = "Test User",
            Email = "test@example.com"
        };

        var result = _controller.Create(registration) as NotFoundResult;

        Assert.NotNull(result);
    }

    [Fact]
    public void Create_Post_FullCapacity_RedirectsWithError()
    {
        var evt = _eventService.Create(new Event
        {
            Title = "Tiny Event",
            Description = "Only 1 spot",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(10).AddHours(2),
            Location = "Small Room",
            Capacity = 1
        });

        _registrationService.Create(new Registration
        {
            EventId = evt.Id,
            Name = "First Person",
            Email = "first@example.com"
        });

        var registration = new Registration
        {
            EventId = evt.Id,
            Name = "Second Person",
            Email = "second@example.com"
        };

        var result = _controller.Create(registration) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Details", result.ActionName);
        Assert.Equal("Events", result.ControllerName);
    }

    [Fact]
    public void Create_Post_SuccessfulRegistration_IncrementsCount()
    {
        var events = _eventService.GetAll().ToList();
        var eventId = events.First().Id;
        var countBefore = _registrationService.GetRegistrationCount(eventId);

        _controller.Create(new Registration
        {
            EventId = eventId,
            Name = "New Attendee",
            Email = "new@example.com"
        });

        var countAfter = _registrationService.GetRegistrationCount(eventId);
        Assert.Equal(countBefore + 1, countAfter);
    }
}
