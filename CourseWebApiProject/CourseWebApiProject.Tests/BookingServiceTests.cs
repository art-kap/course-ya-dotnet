using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using CourseWebApiProject.Services;
using FluentAssertions;
using Moq;

namespace CourseWebApiProject.Tests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockBookingRepository;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _mockBookingRepository = new Mock<IBookingRepository>();
        _mockEventRepository = new Mock<IEventRepository>();
        _bookingService = new BookingService(_mockBookingRepository.Object, _mockEventRepository.Object);
    }

    [Fact]
    public async Task Create_Booking_ShouldCallAddOnce()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.ContainsId(eventId)).Returns(true);

        // Act
        var response = await _bookingService.CreateBookingAsync(eventId);

        // Assert
        _mockBookingRepository.Verify(repo => repo.AddAsync(It.IsAny<Booking>()), Times.Once);
        response.Status.Should().Be((int)BookingStatus.Pending);
    }

    [Fact]
    public async Task Create_TwoBookings_ShouldCreateDifferentIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.ContainsId(eventId)).Returns(true);

        // Act
        var firstResponse = await _bookingService.CreateBookingAsync(eventId);
        var secondResponse = await _bookingService.CreateBookingAsync(eventId);

        // Assert
        _mockBookingRepository.Verify(repo => repo.AddAsync(It.IsAny<Booking>()), Times.Exactly(2));
        firstResponse.Id.Should().NotBe(secondResponse.Id);
    }

    [Fact]
    public async Task Create_NonExistingEvent_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.ContainsId(eventId)).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<EventNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Change_BookingStatus_ShouldReturnActualStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var booking = Booking.Create(eventId);
        var bookingId = booking.Id;

        _mockEventRepository.Setup(repo => repo.ContainsId(eventId)).Returns(true);        
        _mockBookingRepository.Setup(repo => repo.FindByIdAsync(bookingId)).ReturnsAsync(booking);

        // Act
        var initResponse = await _bookingService.GetBookingByIdAsync(bookingId);
        var initStatus = initResponse.Status;

        booking.Confirm();

        var responseAfterConfirm = await _bookingService.GetBookingByIdAsync(bookingId);
        var statusAfterConfirm = responseAfterConfirm.Status;

        booking.Reject();

        var responseAfterReject = await _bookingService.GetBookingByIdAsync(bookingId);
        var statusAfterReject = responseAfterReject.Status;

        // Assert
        initStatus.Should().Be((int)BookingStatus.Pending);
        statusAfterConfirm.Should().Be((int)BookingStatus.Confirmed);
        statusAfterReject.Should().Be((int)BookingStatus.Rejected);
    }

    [Fact]
    public async Task Get_NonExistingId_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.ContainsId(eventId)).Returns(true);
        var response = await _bookingService.CreateBookingAsync(eventId);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<BookingNotFoundException>(() => _bookingService.GetBookingByIdAsync(nonExistingId));
    }
}
