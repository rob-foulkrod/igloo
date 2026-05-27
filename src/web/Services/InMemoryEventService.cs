using web.Models;

namespace web.Services;

public class InMemoryEventService : IEventService
{
    // Not thread-safe — v1 in-memory demo
    private readonly List<Event> _events = new();
    private int _nextId = 1;

    public InMemoryEventService()
    {
        // Seed data per SPEC — dates evaluated at startup
        Create(new Event
        {
            Title = "Community Ice Skating",
            Description = "A fun ice skating event for the whole community. Bring your own skates or rent a pair at the rink.",
            StartDate = DateTime.UtcNow.AddDays(14),
            EndDate = DateTime.UtcNow.AddDays(14).AddHours(3),
            Location = "City Rink",
            Capacity = 50
        });

        Create(new Event
        {
            Title = "Winter Film Festival",
            Description = "Three days of classic winter-themed films shown on the big screen. Popcorn and hot drinks included.",
            StartDate = DateTime.UtcNow.AddDays(21),
            EndDate = DateTime.UtcNow.AddDays(23),
            Location = "Community Theater",
            Capacity = 100
        });

        Create(new Event
        {
            Title = "Hot Cocoa Social",
            Description = "Warm up with friends and neighbors over gourmet hot cocoa and baked treats.",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(7).AddHours(2),
            Location = "Town Hall",
            Capacity = 30
        });
    }

    public IEnumerable<Event> GetAll() => _events.AsReadOnly();

    public Event? GetById(int id) => _events.FirstOrDefault(e => e.Id == id);

    public Event Create(Event evt)
    {
        evt.Id = _nextId++;
        _events.Add(evt);
        return evt;
    }

    public Event Update(Event evt)
    {
        var existing = _events.FirstOrDefault(e => e.Id == evt.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Event with Id {evt.Id} not found.");

        existing.Title = evt.Title;
        existing.Description = evt.Description;
        existing.StartDate = evt.StartDate;
        existing.EndDate = evt.EndDate;
        existing.Location = evt.Location;
        existing.Capacity = evt.Capacity;
        return existing;
    }

    public bool Delete(int id)
    {
        var evt = _events.FirstOrDefault(e => e.Id == id);
        if (evt == null) return false;
        _events.Remove(evt);
        return true;
    }
}
