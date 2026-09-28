using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class BookingService(AppDbContext context) : IBookingService
{
    private readonly AppDbContext _context = context;
    private readonly static SemaphoreSlim CreateBookingSemaphore = new(1, 1);

    public async Task<BookingInfo> CreateBookingAsync(Guid eventId)
    {
        Booking bookingToAdd;

        await CreateBookingSemaphore.WaitAsync();

        try
        {
            var @event = await _context.Events.FindAsync(eventId) ?? throw new EventNotFoundException(eventId);
            var canReserve = @event.TryReserveSeats();

            if (!canReserve)
            {
                throw new NoAvailableSeatsException();
            }

            bookingToAdd = Booking.Create(eventId);
            await _context.Bookings.AddAsync(bookingToAdd);
            await _context.SaveChangesAsync();
        }
        finally
        {
            CreateBookingSemaphore.Release();
        }

        return bookingToAdd.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId)
    {
        var bookingToGet = await _context.Bookings.FindAsync(bookingId) ?? throw new BookingNotFoundException(bookingId);
        return bookingToGet.ToInfo();
    }
}
