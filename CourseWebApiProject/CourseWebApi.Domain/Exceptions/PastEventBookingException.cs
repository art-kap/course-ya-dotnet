namespace CourseWebApi.Domain.Exceptions;

public class PastEventBookingException(Guid eventId) : Exception($"Попытка забронировать прошедшее событие {eventId}.");
