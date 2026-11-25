namespace CourseWebApi.Domain.Exceptions;

public class PendingBookingsLimitException(Guid userId) : Exception($"Превышен лимит активных броней для пользователя {userId}.");
