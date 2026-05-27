using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using web.Controllers;
using web.Models;
using web.Services;

namespace web.tests.Controllers;

public class EventsControllerTests
{
    private EventsController CreateController()
    {
        var eventService = new InMemoryEventService();
        var regService = new InMemoryRegistrationService(eventService);
        var logger = LoggerFactory.Create(b => { }).CreateLogger<EventsController>();
        return new EventsController(eventService, regService, logger);
    }

    [Fact]
    public void Index_ReturnsViewWithEvents()
    {
        var controller = CreateController();

        var result = controller.Index() as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Event>>(result.Model);
        Assert.Equal(3, model.Count());
    }

    [Fact]
    public void Details_ExistingId_ReturnsViewWithEvent()
    {
        var controller = CreateController();

        var result = controller.Details(1) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<Event>(result.Model);
        Assert.Equal("Community Ice Skating", model.Title);
    }

    [Fact]
    public void Details_NonExistingId_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var controller = CreateController();

        var result = controller.Create() as ViewResult;

        Assert.NotNull(result);
    }

    [Fact]
    public void Create_ValidEvent_RedirectsToIndex()
    {
        var controller = CreateController();
        var evt = new Event
        {
            Title = "New Event",
            Description = "Desc",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(10).AddHours(2),
            Location = "Park",
            Capacity = 25
        };

        var result = controller.Create(evt) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public void Create_InvalidModel_ReturnsView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Title", "Required");
        var evt = new Event();

        var result = controller.Create(evt) as ViewResult;

        Assert.NotNull(result);
    }

    [Fact]
    public void Edit_Get_ExistingId_ReturnsViewWithEvent()
    {
        var controller = CreateController();

        var result = controller.Edit(1) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<Event>(result.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public void Edit_Get_NonExistingId_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Edit_Post_ValidEvent_RedirectsToIndex()
    {
        var controller = CreateController();
        var evt = new Event
        {
            Id = 1,
            Title = "Updated",
            Description = "Updated desc",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(10).AddHours(2),
            Location = "New Location",
            Capacity = 60
        };

        var result = controller.Edit(1, evt) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public void Edit_Post_IdMismatch_ReturnsBadRequest()
    {
        var controller = CreateController();
        var evt = new Event { Id = 2 };

        var result = controller.Edit(1, evt);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void Delete_Get_ExistingId_ReturnsViewWithEvent()
    {
        var controller = CreateController();

        var result = controller.Delete(1) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<Event>(result.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public void Delete_Get_NonExistingId_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void DeleteConfirmed_ExistingId_RedirectsToIndex()
    {
        var controller = CreateController();

        var result = controller.DeleteConfirmed(1) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public void DeleteConfirmed_NonExistingId_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.DeleteConfirmed(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
