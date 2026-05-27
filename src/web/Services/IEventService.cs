using web.Models;

namespace web.Services;

public interface IEventService
{
    IEnumerable<Event> GetAll();
    Event? GetById(int id);
    Event Create(Event evt);
    Event Update(Event evt);
    bool Delete(int id);
}
