namespace CourseWebApi.Domain.Exceptions;

public class BookingsLimitExceededException(Guid userId) : Exception($"Превышен лимит активных броней для пользователя {userId}.");
