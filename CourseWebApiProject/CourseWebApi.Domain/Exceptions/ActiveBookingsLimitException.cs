namespace CourseWebApi.Domain.Exceptions;

public class ActiveBookingsLimitException(int limit) : Exception($"Превышен лимит в {limit} активных броней.");
