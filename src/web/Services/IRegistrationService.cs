using web.Models;

namespace web.Services;

public interface IRegistrationService
{
    IEnumerable<Registration> GetByEventId(int eventId);
    Registration? GetById(int id);
    Registration Create(Registration registration);
    bool Delete(int id);
    int GetRegistrationCount(int eventId);
    bool HasCapacity(int eventId);
}
