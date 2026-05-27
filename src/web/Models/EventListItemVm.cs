namespace web.Models;

public class EventListItemVm
{
    public Event Event { get; set; } = default!;
    public int RegistrationCount { get; set; }
    public int RemainingCapacity => Event.Capacity - RegistrationCount;
    public int PercentFull => Event.Capacity > 0
        ? (int)Math.Round(RegistrationCount * 100.0 / Event.Capacity)
        : 0;
}
