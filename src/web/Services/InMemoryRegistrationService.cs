using web.Models;

namespace web.Services;

public class InMemoryRegistrationService : IRegistrationService
{
    // Not thread-safe — v1 in-memory demo
    private readonly List<Registration> _registrations = new();
    private readonly IEventService _eventService;
    private int _nextId = 1;

    public InMemoryRegistrationService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public IEnumerable<Registration> GetByEventId(int eventId) =>
        _registrations.Where(r => r.EventId == eventId).ToList().AsReadOnly();

    public Registration? GetById(int id) =>
        _registrations.FirstOrDefault(r => r.Id == id);

    public Registration Create(Registration registration)
    {
        var evt = _eventService.GetById(registration.EventId)
            ?? throw new KeyNotFoundException($"Event {registration.EventId} not found.");

        if (GetRegistrationCount(evt.Id) >= evt.Capacity)
            throw new InvalidOperationException("Event is at full capacity.");

        registration.Id = _nextId++;
        registration.RegisteredAt = DateTime.UtcNow;
        _registrations.Add(registration);
        return registration;
    }

    public bool Delete(int id)
    {
        var reg = _registrations.FirstOrDefault(r => r.Id == id);
        if (reg == null) return false;
        _registrations.Remove(reg);
        return true;
    }

    public int GetRegistrationCount(int eventId) =>
        _registrations.Count(r => r.EventId == eventId);

    public bool HasCapacity(int eventId)
    {
        var evt = _eventService.GetById(eventId);
        if (evt == null) return false;
        return GetRegistrationCount(eventId) < evt.Capacity;
    }
}
