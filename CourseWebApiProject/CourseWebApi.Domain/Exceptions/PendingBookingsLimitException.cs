namespace CourseWebApi.Domain.Exceptions;

public class PendingBookingsLimitException(int limit) : Exception($"Превышен лимит в {limit} активных броней.");
