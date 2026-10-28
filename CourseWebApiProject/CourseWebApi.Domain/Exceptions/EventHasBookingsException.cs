namespace CourseWebApi.Domain.Exceptions;

public class EventHasBookingsException(Guid eventId) : Exception($"Событие {eventId} нельзя удалить, так как у него есть бронирования.");