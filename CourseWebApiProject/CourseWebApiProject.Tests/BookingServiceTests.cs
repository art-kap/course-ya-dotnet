using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using CourseWebApi.Application.Services;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Domain.Exceptions;
using CourseWebApi.Infrastructure.Persistence;
using CourseWebApi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Concurrent;

namespace CourseWebApiProject.Tests;

public class BookingServiceTests
{
    private readonly ServiceProvider _serviceProvider;

    public BookingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped(_ => Mock.Of<IPasswordHasher>(p => p.Hash(It.IsAny<string>()) == string.Empty));
        services.AddScoped(_ => Mock.Of<IJwtTokenGenerator>());

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Успешное создание бронирования
    /// </summary>
    [Fact]
    public async Task CreateBooking_ExistingEvent_Success()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid eventId, userId;
        int availableSeatsBeforeBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            availableSeatsBeforeBooking = eventInfo.AvailableSeats;
            eventId = eventInfo.Id;
        }

        // Act
        BookingInfo booking;
        EventInfo updatedEvent;

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            booking = await bookingService.CreateBookingAsync(eventId, userId);

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            updatedEvent = await eventService.GetEvent(eventId);
        }

        // Assert
        booking.Should().NotBeNull();
        booking.EventId.Should().Be(eventId);
        booking.Status.Should().Be((int)BookingStatus.Pending);
        updatedEvent.AvailableSeats.Should().Be(availableSeatsBeforeBooking - 1);
    }

    /// <summary>
    /// Успешное создание двух бронирований с разными id
    /// </summary>
    [Fact]
    public async Task Create_TwoBookings_ShouldCreateDifferentIds()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
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
        BookingInfo firstBooking, secondBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            firstBooking = await bookingService.CreateBookingAsync(eventId, userId);
            secondBooking = await bookingService.CreateBookingAsync(eventId, userId);
        }

        // Assert
        firstBooking.Id.Should().NotBe(secondBooking.Id);
    }

    /// <summary>
    /// Успешное создание бронирований до тех пор, пока есть места
    /// </summary>
    [Fact]
    public async Task CreateBookings_ExistingEvent_SuccessUntilNoSeatsLeft()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid eventId, userId;
        int availableSeatsBeforeBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            eventId = eventInfo.Id;
            availableSeatsBeforeBooking = eventInfo.AvailableSeats;
        }

        // Act
        var bookingIdBag = new ConcurrentBag<Guid>();

        for (int i = 0; i < availableSeatsBeforeBooking; i++)
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var response = await bookingService.CreateBookingAsync(eventId, userId);
            bookingIdBag.Add(response.Id);
        }

        // Assert
        bookingIdBag.Distinct().Count().Should().Be(availableSeatsBeforeBooking);

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(eventId, userId));
        }
    }

    /// <summary>
    /// Неудача при попытке создать бронирование на несуществующее событие
    /// </summary>
    [Fact]
    public async Task CreateBooking_NonExistingEvent_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid userId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
        }

        var nonExistingEventId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(nonExistingEventId, userId));
        }
    }

    /// <summary>
    /// Неудача при попытке создать бронирование на удаленное событие
    /// </summary>
    [Fact]
    public async Task CreateBooking_RemovedEvent_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
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
            await eventService.RemoveEvent(eventId);
        }

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(eventId, userId));
        }
    }

    /// <summary>
    /// Неудача при попытке получить бронирование по несуществущему id
    /// </summary>
    [Fact]
    public async Task Get_NonExistingId_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
        }

        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.GetBookingByIdAsync(nonExistingId));
        }
    }

    /// <summary>
    /// Проверяет обработку бронирования
    /// </summary>
    [Fact]
    public async Task ProcessBooking_ExistingEvent_ShouldConfirmBooking()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid bookingId, userId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(eventInfo.Id, userId);
            bookingId = booking.Id;
        }

        // Act
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.TryConfirmBookingAsync(bookingId);
        }

        BookingInfo bookingToConfirm;

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            bookingToConfirm = await bookingService.GetBookingByIdAsync(bookingId);
        }

        // Assert
        bookingToConfirm.Status.Should().Be((int)BookingStatus.Confirmed);
    }

    /// <summary>
    /// Неудача при обработке несуществующего бронирования
    /// </summary>
    [Fact]
    public async Task Confirm_NonExistingBooking_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid userId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.CreateBookingAsync(eventInfo.Id, userId);
        }

        var nonExistingBookingId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.TryConfirmBookingAsync(nonExistingBookingId));
        }
    }

    /// <summary>
    /// Проверка отклонения брони и откат количества доступных мест
    /// </summary>
    [Fact]
    public async Task ConfirmAndReject_Booking_ShouldReleaseSeats()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid bookingId, eventId, userId;
        int availableSeatsBeforeBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            availableSeatsBeforeBooking = eventInfo.AvailableSeats;
            eventId = eventInfo.Id;

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(eventId, userId);
            bookingId = booking.Id;

            await bookingService.TryConfirmBookingAsync(bookingId);
        }

        // Act
        int availableSeatsAfterReject;

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.RejectBookingAsync(bookingId);

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.GetEvent(eventId);
            availableSeatsAfterReject = eventInfo.AvailableSeats;
        }

        // Assert
        availableSeatsAfterReject.Should().Be(availableSeatsBeforeBooking);
    }

    /// <summary>
    /// Неудача при попытке получить бронирование для события, которое уже началось
    /// </summary>
    [Fact]
    public async Task TryBooking_PastEvent_ShouldThrowPastEventBookingException()
    {
        // Arrange
        var pastEventDto = EventsTestsHelper.GetPastEventCreate();
        var userDto = EventsTestsHelper.GetTestUserCreate();

        Guid eventId, userId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userInfo = await userService.AddUser(userDto);
            userId = userInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(pastEventDto);
            eventId = eventInfo.Id;
        }

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<PastEventBookingException>(() => bookingService.CreateBookingAsync(eventId, userId));
        }
    }

    /// <summary>
    /// Успешная отмена чужого бронирования с правами админа
    /// </summary>
    [Fact]
    public async Task CancelOtherUserBooking_Admin_ShouldCancelBooking()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var bookedUserDto = EventsTestsHelper.GetTestUserCreate();
        var adminDto = EventsTestsHelper.GetTestAdminCreate();

        Guid eventId, bookingId, adminId;
        int availableSeatsBeforeBooking, availableSeatsAfterBooking, availableSeatsAfterCancelling;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var bookedUserInfo = await userService.AddUser(bookedUserDto);
            var adminInfo = await userService.AddUser(adminDto);
            adminId = adminInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            eventId = eventInfo.Id;
            availableSeatsBeforeBooking = eventInfo.AvailableSeats;

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(eventId, bookedUserInfo.Id);
            bookingId = booking.Id;
        }

        // Act
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfoAfterBooking = await eventService.GetEvent(eventId);
            availableSeatsAfterBooking = eventInfoAfterBooking.AvailableSeats;

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.CancelBooking(bookingId, adminId);
        }

        BookingInfo cancelledBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfoAfterCancelling = await eventService.GetEvent(eventId);
            availableSeatsAfterCancelling = eventInfoAfterCancelling.AvailableSeats;

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            cancelledBooking = await bookingService.GetBookingByIdAsync(bookingId);
        }

        // Assert
        cancelledBooking.Status.Should().Be((int)BookingStatus.Cancelled);
        availableSeatsAfterBooking.Should().Be(availableSeatsBeforeBooking - 1);
        availableSeatsAfterCancelling.Should().Be(availableSeatsBeforeBooking);
    }

    /// <summary>
    /// Неудача при попытке отменить чужое бронирование без прав админа
    /// </summary>
    [Fact]
    public async Task CancelOtherUserBooking_User_ShouldCancelBooking()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();
        var bookedUserDto = EventsTestsHelper.GetTestUserCreate("first");
        var cancellingUserDto = EventsTestsHelper.GetTestUserCreate("second");

        Guid bookingId, cancellingUserId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var bookedUserInfo = await userService.AddUser(bookedUserDto);
            var cancellingUserInfo = await userService.AddUser(cancellingUserDto);
            cancellingUserId = cancellingUserInfo.Id;

            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(eventInfo.Id, bookedUserInfo.Id);
            bookingId = booking.Id;
        }

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<AccessDeniedException>(() => bookingService.CancelBooking(bookingId, cancellingUserId));
        }
    }

    /// <summary>
    /// Проверяет лимит пользователя на кол-во активных бронирований
    /// </summary>
    [Fact]
    public async Task CreateBooking_LimitExceed_ShouldThrowActiveBookingsLimitException()
    {
        // Arrange
        const int totalSeats = EventsTestsHelper.ActiveBookingsLimit + 1; // кол-во мест задаем с запасом
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
        for (int i = 0; i < EventsTestsHelper.ActiveBookingsLimit; i++)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.CreateBookingAsync(eventId, userId);
        }

        // Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<ActiveBookingsLimitException>(() => bookingService.CreateBookingAsync(eventId, userId));
        }
    }
}
