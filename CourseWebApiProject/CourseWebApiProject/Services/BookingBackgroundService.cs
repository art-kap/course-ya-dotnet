using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.Services;

public class BookingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис начал работу.");

        while (!stoppingToken.IsCancellationRequested)
        {
            IReadOnlyCollection<Booking> pendingBookings;

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                pendingBookings = await context.Bookings.Where(b => b.Status == BookingStatus.Pending).ToListAsync(stoppingToken);
            }

            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking.Id, stoppingToken));

            await Task.WhenAll(tasks);
            await Task.Delay(PollingInterval, stoppingToken);
        }

        _logger.LogInformation("Фоновый сервис завершает работу.");
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _logger.LogInformation($"Начато оформление бронирования {bookingId}");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Имитация обработки бронирования
        await Task.Delay(ProcessingDelay, stoppingToken);

        var booking = await context.Bookings.FindAsync(bookingId, stoppingToken) ?? throw new BookingNotFoundException(bookingId);
        var @event = await context.Events.FindAsync(booking.EventId, stoppingToken);

        try
        {
            if (@event == null)
            {
                booking.Reject();
                _logger.LogWarning($"Бронирование {booking.Id} отклонено. Не найдено событие {booking.EventId}.");
            }
            else
            {
                booking.Confirm();
                _logger.LogInformation($"Бронирование {booking.Id} оформлено.");
            }

            await context.SaveChangesAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Штатная остановка, выходим из цикла
            _logger.LogInformation($"Бронирование {booking.Id} отменено.");
        }
        catch (Exception e)
        {
            booking.Reject();
            @event?.ReleaseSeats();
            await context.SaveChangesAsync(stoppingToken);

            _logger.LogError(e, $"Возникла непредвиденная ошибка при оформлении бронирования {booking.Id}.");
        }
    }
}
