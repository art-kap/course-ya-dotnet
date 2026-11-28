namespace CourseWebApi.Domain.Exceptions;

public class BookingIsAlreadyRejectedException(Guid bookingId) : Exception($"Бронирование {bookingId} было отклонено ранее.");
