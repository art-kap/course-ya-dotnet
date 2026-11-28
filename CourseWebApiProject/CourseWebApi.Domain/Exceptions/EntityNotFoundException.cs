namespace CourseWebApi.Domain.Exceptions;

public class EntityNotFoundException(string entityName, Guid eventId) : Exception($"Сущность '{entityName}' с id={eventId} не найдена.");
