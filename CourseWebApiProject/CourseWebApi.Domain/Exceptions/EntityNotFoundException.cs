namespace CourseWebApi.Domain.Exceptions;

public class EntityNotFoundException(string entityName, Guid eventId) : Exception($"{entityName} {eventId} не найдено.");
