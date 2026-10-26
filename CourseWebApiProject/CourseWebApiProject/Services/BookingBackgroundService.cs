using CourseWebApi.Application.Interfaces;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Domain.Exceptions;

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
                var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                pendingBookings = await bookingStore.GetPending();
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
        var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        // Имитация обработки бронирования
        await Task.Delay(ProcessingDelay, stoppingToken);

        var booking = await bookingStore.FindAsync(bookingId, stoppingToken) ?? throw new BookingNotFoundException(bookingId);
        var @event = await eventStore.FindAsync(booking.EventId, stoppingToken);

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

            await bookingStore.SaveChangesAsync();
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

            await bookingStore.SaveChangesAsync();

            _logger.LogError(e, $"Возникла непредвиденная ошибка при оформлении бронирования {booking.Id}.");
        }
    }
}
