namespace CourseWebApi.Domain.Exceptions;

public class EntityNotFoundException(string entityName, Guid eventId) : Exception($"{entityName} с id={eventId} не найдено.")
{
}
