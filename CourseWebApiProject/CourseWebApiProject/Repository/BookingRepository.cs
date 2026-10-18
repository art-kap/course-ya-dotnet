using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.Repository;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly AppDbContext _context = context;

    public async Task Add(Booking booking, CancellationToken ct = default)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Booking?> FindAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _context.Bookings.FindAsync(bookingId, ct);
    }

    public async Task<IReadOnlyCollection<Booking>> GetAll(CancellationToken ct = default)
    {
        return await _context.Bookings.ToListAsync();
    }

    public async Task<IReadOnlyCollection<Booking>> GetPending(CancellationToken ct = default)
    {
        return await _context.Bookings.Where(b => b.Status == BookingStatus.Pending).ToListAsync();
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
