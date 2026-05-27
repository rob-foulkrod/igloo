using Microsoft.AspNetCore.Mvc;
using web.Models;
using web.Services;

namespace web.Controllers;

public class EventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventService eventService,
        IRegistrationService registrationService,
        ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _registrationService = registrationService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var events = _eventService.GetAll();
        return View(events);
    }

    public IActionResult Details(int id)
    {
        var evt = _eventService.GetById(id);
        if (evt == null)
        {
            _logger.LogWarning("Event {EventId} not found", id);
            return NotFound();
        }

        ViewBag.RegistrationCount = _registrationService.GetRegistrationCount(id);
        ViewBag.RemainingCapacity = evt.Capacity - _registrationService.GetRegistrationCount(id);
        return View(evt);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Event evt)
    {
        if (!ModelState.IsValid)
        {
            return View(evt);
        }

        _eventService.Create(evt);
        _logger.LogInformation("Created event {EventTitle} with Id {EventId}", evt.Title, evt.Id);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var evt = _eventService.GetById(id);
        if (evt == null)
        {
            return NotFound();
        }

        return View(evt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Event evt)
    {
        if (id != evt.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(evt);
        }

        try
        {
            _eventService.Update(evt);
            _logger.LogInformation("Updated event {EventId}", evt.Id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var evt = _eventService.GetById(id);
        if (evt == null)
        {
            return NotFound();
        }

        return View(evt);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var result = _eventService.Delete(id);
        if (!result)
        {
            return NotFound();
        }

        _logger.LogInformation("Deleted event {EventId}", id);
        return RedirectToAction(nameof(Index));
    }
}
