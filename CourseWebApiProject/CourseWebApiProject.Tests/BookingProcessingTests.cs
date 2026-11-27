using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using CourseWebApi.Application.Services;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Domain.Exceptions;
using CourseWebApi.Infrastructure.Persistence;
using CourseWebApi.Infrastructure.Persistence.Repositories;
using CourseWebApi.Infrastructure.Services;
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
        services.AddSingleton(Mock.Of<ILogger<BookingBackgroundService>>());

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped(_ => Mock.Of<IPasswordHasher>(p => p.Hash(It.IsAny<string>()) == string.Empty));
        services.AddScoped(_ => Mock.Of<IJwtTokenGenerator>());

        services.AddHostedService<BookingBackgroundService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Проверяет работу фонового сервиса
    /// </summary>
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
        var userDto = EventsTestsHelper.GetTestUserCreate();

        BookingInfo booking;
        EventInfo eventInfo;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            eventInfo = await eventService.AddEvent(validEventDto);
            booking = await bookingService.CreateBookingAsync(eventInfo.Id, userInfo.Id);
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

    /// <summary>
    /// Проверяет обработку бронирований при овербукинге
    /// </summary>
    [Fact]
    public async Task ConcurrentBookings_Overbooking_CorrectProcessing()
    {
        // Arrange
        const int totalSeats = 5;
        const int concurrentBookings = 20;
        var validEventDto = EventsTestsHelper.GetValidEventCreate(totalSeats);
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid eventId, userId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            eventId = eventInfo.Id;
        }

        // Act
        var tasks = Enumerable.Range(0, concurrentBookings)
            .Select(async _ =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    await bookingService.CreateBookingAsync(eventId, userId);

                    return true;
                }
                catch (NoAvailableSeatsException)
                {
                    return false;
                }
                catch (ActiveBookingsLimitException)
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

    /// <summary>
    /// Проверяет создание бронирований с уникальными id
    /// </summary>
    [Fact]
    public async Task ConcurrentBookings_BackgroundService_UniqueConfirmedBookings()
    {
        // Arrange
        const int totalSeats = 10;
        const int concurrentBookings = totalSeats;
        var validEventDto = EventsTestsHelper.GetValidEventCreate(totalSeats);
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid eventId, userId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfoBeforeBooking = await eventService.AddEvent(validEventDto);
            eventId = eventInfoBeforeBooking.Id;
        }

        var successedBookingIdBag = new ConcurrentBag<Guid>();

        // Act
        var tasks = Enumerable.Range(0, concurrentBookings)
           .Select(_ => Task.Run(async () =>
           {
               try
               {
                   using var scope = _serviceProvider.CreateScope();
                   var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                   var response = await bookingService.CreateBookingAsync(eventId, userId);
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

    /// <summary>
    /// Проверяет лимит пользователя на кол-во активных бронирований (конкурентность)
    /// </summary>
    [Fact]
    public async Task ConcurrentBookings_SingleUser_ShouldLimitActiveBookings()
    {
        // Arrange
        const int totalSeats = EventsTestsHelper.ActiveBookingsLimit * 3;
        const int concurrentBookings = EventsTestsHelper.ActiveBookingsLimit * 2;
        var validEventDto = EventsTestsHelper.GetValidEventCreate(totalSeats);
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid eventId, userId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            eventId = eventInfo.Id;
        }

        // Act
        var tasks = Enumerable.Range(0, concurrentBookings)
            .Select(async _ =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    await bookingService.CreateBookingAsync(eventId, userId);

                    return true;
                }
                catch (ActiveBookingsLimitException)
                {
                    return false;
                }
            }).ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Count(r => r).Should().Be(EventsTestsHelper.ActiveBookingsLimit);
    }

    /// <summary>
    /// Проверяет независимость лимитов на кол-во активных бронирований для разных пользователей (конкурентность)
    /// </summary>
    [Fact]
    public async Task ConcurrentBookings_MultipleUsers_ShouldLimitActiveBookingsIndependently()
    {
        // Arrange
        const int concurrentBookingsPerUser = 15;
        const int usersCount = 5;
        const int totalSeats = usersCount * EventsTestsHelper.ActiveBookingsLimit;

        var validEventDto = EventsTestsHelper.GetValidEventCreate(totalSeats);
        Guid eventId;

        var userIdBag = new ConcurrentBag<Guid>();

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            for (int i = 0; i < usersCount; i++)
            {
                var userDto = EventsTestsHelper.GetTestUserCreate($"user_{i}");
                var userInfo = await userService.AddUser(userDto);
                userIdBag.Add(userInfo.Id);
            }

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            eventId = eventInfo.Id;
        }

        // Act
        var tasks = Enumerable.Range(0, usersCount * concurrentBookingsPerUser)
            .Select(async taskId =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    await bookingService.CreateBookingAsync(eventId, userIdBag.ElementAt(taskId % usersCount));

                    return true;
                }
                catch (ActiveBookingsLimitException)
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
        results.Count(r => r).Should().Be(usersCount * EventsTestsHelper.ActiveBookingsLimit);
        response.AvailableSeats.Should().Be(0);
    }
}
