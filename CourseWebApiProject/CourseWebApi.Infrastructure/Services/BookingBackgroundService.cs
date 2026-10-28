using CourseWebApi.Application.Interfaces;
using CourseWebApi.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CourseWebApi.Infrastructure.Services;

public class BookingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);

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

        try
        {
            _logger.LogInformation($"Начато оформление бронирования {bookingId}");

            using var scope = _scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            await bookingService.ProcessBookingAsync(bookingId, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Штатная остановка, выходим из цикла
            _logger.LogInformation($"Бронирование {bookingId} отменено.");
        }
        catch (Exception e)
        {

            _logger.LogError(e, $"Возникла непредвиденная ошибка при оформлении бронирования {bookingId}.");
        }
    }
}
