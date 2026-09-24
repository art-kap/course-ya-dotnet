using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using System.Collections.Concurrent;

namespace CourseWebApiProject.Services;

public class BookingBackgroundService(IBookingRepository bookingStore, IEventRepository eventStore, ILogger<BookingBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

    private readonly IBookingRepository _bookingStore = bookingStore;
    private readonly IEventRepository _eventStore = eventStore;
    private readonly ILogger _logger = logger;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _eventSemaphores = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис начал работу.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingBookings = await _bookingStore.GetPendingAsync();
            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

            await Task.WhenAll(tasks);
            await Task.Delay(PollingInterval, stoppingToken);
        }

        _logger.LogInformation("Фоновый сервис завершает работу.");
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _logger.LogInformation($"Начато оформление бронирования {booking.Id}");

        // Имитация обработки бронирования
        await Task.Delay(ProcessingDelay, stoppingToken);

        var semaphore = _eventSemaphores.GetOrAdd(booking.EventId, k => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(stoppingToken);

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
        catch (Exception e)
        {
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
            semaphore.Release();
        }
    }
}
