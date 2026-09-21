using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class BookingBackgroundService : BackgroundService
{
    private readonly IBookingRepository _bookingStore;
    private readonly IEventRepository _eventStore;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public BookingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger)
    {
        using var scope = scopeFactory.CreateScope();

        _bookingStore = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        _eventStore = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис начал работу.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingBookings = await _bookingStore.GetPendingAsync();
            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

            await Task.WhenAll(tasks);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("Фоновый сервис завершает работу.");
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _logger.LogInformation($"Начато оформление бронирования {booking.Id}");

        // Имитация обработки бронирования
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        await _processingSemaphore.WaitAsync(stoppingToken);

        var @event = _eventStore.FindById(booking.EventId);

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

            await _bookingStore.UpdateAsync(booking);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Штатная остановка, выходим из цикла
            _logger.LogInformation($"Бронирование {booking.Id} отменено.");
        }
        catch (Exception e) {
            booking.Reject();
            await _bookingStore.UpdateAsync(booking);

            if (@event != null)
            {
                @event.ReleaseSeats();
                _eventStore.Update(@event);
            }

            _logger.LogError(e, $"Возникла непредвиденная ошибка при оформлении бронирования {booking.Id}.");
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}
