using CourseWebApiProject.Repository;
using FluentAssertions;

namespace CourseWebApiProject.Tests;

public class InMemoryEventStoreTests
{
    private readonly InMemoryEventStore _inMemoryEventStore;

    public InMemoryEventStoreTests()
    {
        _inMemoryEventStore = new InMemoryEventStore();
    }

    [Fact]
    public void Find_ExistingId_Success()
    {
        // Arrange
        var eventToAdd = EventsTestsHelper.GetValidEvent();
        _inMemoryEventStore.Add(eventToAdd);

        // Act
        var response = _inMemoryEventStore.FindById(eventToAdd.Id);

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public void Find_NonExistingId_Fail()
    {
        // Arrange
        var eventToAdd = EventsTestsHelper.GetValidEvent();
        _inMemoryEventStore.Add(eventToAdd);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = _inMemoryEventStore.FindById(nonExistingId);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void Update_ExistingId_Success()
    {
        // Arrange
        var eventToAdd = EventsTestsHelper.GetValidEvent();
        var id = eventToAdd.Id;
        _inMemoryEventStore.Add(eventToAdd);
        var eventDataToUpdate = EventsTestsHelper.GetAnotherValidEvent(id);

        // Act
        _inMemoryEventStore.Update(eventDataToUpdate);
        var response = _inMemoryEventStore.FindById(id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(eventDataToUpdate);
    }

    [Fact]
    public void Update_NonExistingId_Fail()
    {
        // Arrange
        var eventToAdd = EventsTestsHelper.GetValidEvent();
        var existingId = eventToAdd.Id;
        var nonExistingId = Guid.NewGuid();
        _inMemoryEventStore.Add(eventToAdd);
        var eventDataToUpdate = EventsTestsHelper.GetAnotherValidEvent(nonExistingId);

        // Act
        _inMemoryEventStore.Update(eventDataToUpdate);
        var nonExistingIdResponse = _inMemoryEventStore.FindById(nonExistingId);
        var oldIdResponse = _inMemoryEventStore.FindById(existingId);

        // Assert
        nonExistingIdResponse.Should().BeNull();
        oldIdResponse.Should().BeEquivalentTo(eventToAdd);
    }

    [Fact]
    public void Remove_ExistingId_Success()
    {
        // Arrange
        var eventToAdd = EventsTestsHelper.GetValidEvent();
        _inMemoryEventStore.Add(eventToAdd);

        // Act
        var removeResponse = _inMemoryEventStore.RemoveById(eventToAdd.Id);
        var findResponse = _inMemoryEventStore.FindById(eventToAdd.Id);

        // Assert
        removeResponse.Should().BeTrue();
        findResponse.Should().BeNull();
    }

    [Fact]
    public void Remove_NonExistingId_Fail()
    {
        // Arrange
        var eventToAdd = EventsTestsHelper.GetValidEvent();
        _inMemoryEventStore.Add(eventToAdd);
        var nonExistingId = Guid.NewGuid();

        // Act
        var removeResponse = _inMemoryEventStore.RemoveById(nonExistingId);

        // Assert
        removeResponse.Should().BeFalse();
    }
}
