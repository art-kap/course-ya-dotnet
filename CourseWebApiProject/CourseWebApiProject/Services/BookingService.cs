using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly object _bookingLock = new();

    public async Task<BookingInfo> CreateBookingAsync(Guid eventId)
    {
        Booking bookingToAdd;

        lock (_bookingLock)
        {
            var @event = _eventRepository.FindById(eventId) ?? throw new EventNotFoundException(eventId);
            var canReserve = @event.TryReserveSeats();

            if (!canReserve)
            {
                throw new NoAvailableSeatsException();
            }

            bookingToAdd = Booking.Create(eventId);
            _bookingRepository.AddAsync(bookingToAdd);
        }

        return bookingToAdd.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId)
    {
        var bookingToGet = await _bookingRepository.FindByIdAsync(bookingId);
        return bookingToGet == null ? throw new BookingNotFoundException(bookingId) : bookingToGet.ToInfo();
    }
}
