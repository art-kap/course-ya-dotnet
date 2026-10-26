namespace CourseWebApi.Domain.Exceptions;

public class EventNotFoundException(Guid eventId) : EntityNotFoundException("Событие", eventId)
{
}
