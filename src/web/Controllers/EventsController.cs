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
        var viewModel = events.Select(e => new EventListItemVm
        {
            Event = e,
            RegistrationCount = _registrationService.GetRegistrationCount(e.Id)
        });
        return View(viewModel);
    }

    public IActionResult Details(int id)
    {
        var evt = _eventService.GetById(id);
        if (evt == null)
        {
            _logger.LogWarning("Event {EventId} not found", id);
            return NotFound();
        }

        var registrationCount = _registrationService.GetRegistrationCount(id);
        ViewBag.RegistrationCount = registrationCount;
        ViewBag.RemainingCapacity = evt.Capacity - registrationCount;
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
