using CourseWebApiProject.Models;
using CourseWebApiProject.Repository;
using FluentAssertions;

namespace CourseWebApiProject.Tests;

public class InMemoryBookingStoreTests
{
    private readonly InMemoryBookingStore _inMemoryBookingStore;

    public InMemoryBookingStoreTests()
    {
        _inMemoryBookingStore = new InMemoryBookingStore();
    }

    [Fact]
    public async Task Find_ExistingId_Success()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var bookingToAdd = Booking.Create(eventId);
        await _inMemoryBookingStore.AddAsync(bookingToAdd);

        // Act
        var response = await _inMemoryBookingStore.FindByIdAsync(bookingToAdd.Id);

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Find_NonExistingId_Fail()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var bookingToAdd = Booking.Create(eventId);
        await _inMemoryBookingStore.AddAsync(bookingToAdd);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _inMemoryBookingStore.FindByIdAsync(nonExistingId);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task Update_ExistingId_Success()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var bookingToAdd = Booking.Create(eventId);
        var bookingId = bookingToAdd.Id;
        var createdAt = bookingToAdd.CreatedAt;

        await _inMemoryBookingStore.AddAsync(bookingToAdd);
        bookingToAdd.Confirm();

        // Act
        await _inMemoryBookingStore.UpdateAsync(bookingToAdd);
        var response = await _inMemoryBookingStore.FindByIdAsync(bookingId);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(bookingId);
        response.EventId.Should().Be(eventId);
        response.Status.Should().Be(BookingStatus.Confirmed);
        response.CreatedAt.Should().Be(createdAt);
        response.ProcessedAt.Should().BeAfter(createdAt);
    }

    [Fact]
    public async Task Update_NonExistingId_Fail()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var bookingToAdd = Booking.Create(eventId);
        var existingId = bookingToAdd.Id;
        await _inMemoryBookingStore.AddAsync(bookingToAdd);

        var bookingToUpdate = Booking.Create(eventId);
        var nonExistingId = bookingToUpdate.Id;

        // Act
        await _inMemoryBookingStore.UpdateAsync(bookingToUpdate);
        var nonExistingIdResponse = await _inMemoryBookingStore.FindByIdAsync(nonExistingId);
        var oldIdResponse = await _inMemoryBookingStore.FindByIdAsync(existingId);

        // Assert
        nonExistingIdResponse.Should().BeNull();
        oldIdResponse.Should().BeEquivalentTo(bookingToAdd);
    }
}
