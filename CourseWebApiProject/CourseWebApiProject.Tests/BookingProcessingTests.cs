using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using CourseWebApiProject.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;

namespace CourseWebApiProject.Tests;

public class BookingProcessingTests
{
    private readonly ServiceProvider _serviceProvider;

    public BookingProcessingTests()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddSingleton(new Mock<ILogger<BookingBackgroundService>>().Object);
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddHostedService<BookingBackgroundService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Wait_BackgroundConfirmation_ShouldConfirmBooking()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var backgroundService = scope.ServiceProvider.GetRequiredService<IHostedService>();
            await backgroundService.StartAsync(CancellationToken.None);
        }

        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        BookingInfo booking;
        EventInfo eventInfo;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            eventInfo = await eventService.AddEvent(validEventDto);
            booking = await bookingService.CreateBookingAsync(eventInfo.Id);
        }

        var bookingId = booking.Id;

        var serviceWorktime = TimeSpan.FromSeconds(5);

        // Act
        await Task.Delay(serviceWorktime);

        using (var scope = _serviceProvider.CreateScope())
        {
            var backgroundService = scope.ServiceProvider.GetRequiredService<IHostedService>();
            await backgroundService.StopAsync(CancellationToken.None);
        }

        BookingInfo handledBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            handledBooking = await bookingService.GetBookingByIdAsync(bookingId);
        }

        // Assert
        handledBooking.Id.Should().Be(bookingId);
        handledBooking.EventId.Should().Be(eventInfo.Id);
        handledBooking.Status.Should().Be((int)BookingStatus.Confirmed);
    }

    [Fact]
    public async Task ConcurrentBookings_Overbooking_CorrectProcessing()
    {
        // Arrange
        const int totalSeats = 5;
        const int concurrentBookings = 20;
        var validEventDto = EventsTestsHelper.GetValidEventCreate(totalSeats);

        EventInfo eventInfo;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            eventInfo = await eventService.AddEvent(validEventDto);
        }

        var eventId = eventInfo.Id;

        // Act
        var tasks = Enumerable.Range(0, concurrentBookings)
            .Select(async _ =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    await bookingService.CreateBookingAsync(eventId);

                    return true;
                }
                catch (NoAvailableSeatsException)
                {
                    return false;
                }
            }).ToArray();

        var results = await Task.WhenAll(tasks);

        EventInfo response;
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            response = await eventService.GetEvent(eventId);
        }

        // Assert
        results.Count(r => r).Should().Be(totalSeats);
        response.AvailableSeats.Should().Be(0);
    }

    [Fact]
    public async Task ConcurrentBookings_BackgroundService_UniqueConfirmedBookings()
    {
        // Arrange
        const int totalSeats = 10;
        const int concurrentBookings = totalSeats;
        var validEventDto = EventsTestsHelper.GetValidEventCreate(totalSeats);

        EventInfo eventInfoBeforeBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            eventInfoBeforeBooking = await eventService.AddEvent(validEventDto);
        }

        var eventId = eventInfoBeforeBooking.Id;

        var successedBookingIdBag = new ConcurrentBag<Guid>();

        // Act
        var tasks = Enumerable.Range(0, concurrentBookings)
           .Select(_ => Task.Run(async () =>
           {
               try
               {
                   using var scope = _serviceProvider.CreateScope();
                   var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                   var response = await bookingService.CreateBookingAsync(eventId);
                   successedBookingIdBag.Add(response.Id);
                   return true;
               }
               catch (NoAvailableSeatsException)
               {
                   return false;
               }
           })).ToArray();

        var results = await Task.WhenAll(tasks);

        EventInfo eventInfoAfterBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = _serviceProvider.GetRequiredService<IEventService>();
            eventInfoAfterBooking = await eventService.GetEvent(eventId);
        }

        // Assert
        successedBookingIdBag.Distinct().Count().Should().Be(concurrentBookings);
        eventInfoAfterBooking.AvailableSeats.Should().Be(0);
    }
}
