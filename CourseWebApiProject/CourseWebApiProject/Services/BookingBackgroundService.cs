using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class BookingBackgroundService(IServiceScopeFactory scopeFactory, 
    ILogger<BookingBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис начал работу.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var bookings = await bookingRepository.GetAllAsync();
                var pendingBookings = bookings.Where(b => b.Status == BookingStatus.Pending);

                foreach (var booking in pendingBookings)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    _logger.LogInformation($"Начато оформление бронирования {booking.Id}");

                    // Имитация обработки бронирования
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                    booking.Confirm();
                    await bookingRepository.UpdateAsync(booking);

                    _logger.LogInformation($"Бронирование {booking.Id} оформлено.");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Штатная остановка, выходим из цикла
                _logger.LogInformation("Бронирование отменено.");
                break;
            }
            catch (Exception ex)
            {
                // Продолжаем цикл после паузы
                _logger.LogError(ex, "Ошибка при обработке задачи");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("Фоновый сервис завершает работу.");
    }
}
