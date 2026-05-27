using Microsoft.AspNetCore.Mvc;
using web.Models;
using web.Services;

namespace web.Controllers;

public class RegistrationsController : Controller
{
    private readonly IRegistrationService _registrationService;
    private readonly IEventService _eventService;
    private readonly ILogger<RegistrationsController> _logger;

    public RegistrationsController(
        IRegistrationService registrationService,
        IEventService eventService,
        ILogger<RegistrationsController> logger)
    {
        _registrationService = registrationService;
        _eventService = eventService;
        _logger = logger;
    }

    public IActionResult Create(int eventId)
    {
        var evt = _eventService.GetById(eventId);
        if (evt == null)
        {
            _logger.LogWarning("Event {EventId} not found for registration", eventId);
            return NotFound();
        }

        if (!_registrationService.HasCapacity(eventId))
        {
            _logger.LogInformation("Event {EventId} is at full capacity", eventId);
            TempData["Error"] = "This event is at full capacity.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        ViewBag.EventTitle = evt.Title;
        var registration = new Registration { EventId = eventId };
        return View(registration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Registration registration)
    {
        if (!ModelState.IsValid)
        {
            var evt = _eventService.GetById(registration.EventId);
            ViewBag.EventTitle = evt?.Title ?? "Unknown Event";
            return View(registration);
        }

        try
        {
            _registrationService.Create(registration);
            _logger.LogInformation("Registration created for Event {EventId} by {Email}",
                registration.EventId, registration.Email);
            TempData["Success"] = "You have been successfully registered!";
            return RedirectToAction("Details", "Events", new { id = registration.EventId });
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Event {EventId} not found during registration", registration.EventId);
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("Capacity exceeded for Event {EventId}", registration.EventId);
            TempData["Error"] = "This event is at full capacity.";
            return RedirectToAction("Details", "Events", new { id = registration.EventId });
        }
    }
}
