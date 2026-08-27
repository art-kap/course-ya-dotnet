namespace CourseWebApiProject.Exceptions;

public class EventNotFoundException(Guid eventId) : EntityNotFoundException("Событие", eventId)
{
}
