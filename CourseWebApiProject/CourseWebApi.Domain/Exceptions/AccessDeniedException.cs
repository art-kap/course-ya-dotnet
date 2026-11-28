namespace CourseWebApi.Domain.Exceptions;

public class AccessDeniedException(Guid userId) : Exception($"Отсутствуют права на операцию у пользователя {userId}.");
