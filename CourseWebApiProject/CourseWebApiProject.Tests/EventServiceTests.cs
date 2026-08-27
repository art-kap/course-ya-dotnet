using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;
using CourseWebApiProject.Services;
using Moq;

namespace CourseWebApiProject.Tests;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _mockRepository = new Mock<IEventRepository>();
        _eventService = new EventService(_mockRepository.Object);
    }

    [Fact]
    public void Add_Event_ShouldCallAddOnce()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();

        // Act
        var response = _eventService.AddEvent(validEvent);

        // Assert
        _mockRepository.Verify(repo => repo.Add(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public void Add_EventWithInvalidDates_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEvent = EventsTestsHelper.GetEventDtoWithInvalidDates();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _eventService.AddEvent(invalidEvent));
    }

    [Fact]
    public void Get_ExistingId_ShouldCallFindByIdOnce()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEvent();
        var id = validEvent.Id;
        _mockRepository.Setup(repo => repo.FindById(id)).Returns(validEvent);

        // Act
        var response = _eventService.GetEvent(id);

        // Assert
        _mockRepository.Verify(repo => repo.FindById(id), Times.Once);
    }

    [Fact]
    public void Get_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        _eventService.AddEvent(validEvent);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.GetEvent(nonExistingId));
    }

    [Fact]
    public void Update_ExistingId_ShouldCallUpdateOnce()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEvent();
        var id = validEvent.Id;
        var anotherValidEventDto = EventsTestsHelper.GetAnotherValidEventDto();

        _mockRepository.Setup(repo => repo.FindById(id)).Returns(validEvent);

        // Act
        _eventService.UpdateEvent(id, anotherValidEventDto);

        // Assert
        _mockRepository.Verify(repo => repo.Update(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public void Update_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        _eventService.AddEvent(validEvent);
        var anotherValidEvent = EventsTestsHelper.GetAnotherValidEventDto();
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.UpdateEvent(nonExistingId, anotherValidEvent));
    }

    [Fact]
    public void Remove_ExistingId_ShouldCallRemoveOnce()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        var id = _eventService.AddEvent(validEvent).Id;

        _mockRepository.Setup(repo => repo.RemoveById(id)).Returns(true);

        // Act
        _eventService.RemoveEvent(id);

        // Assert
        _mockRepository.Verify(repo => repo.RemoveById(id), Times.Once);
    }

    [Fact]
    public void Remove_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        _eventService.AddEvent(validEvent);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.RemoveEvent(nonExistingId));
    }

    [Fact]
    public void Update_InvalidDates_ShouldThrowArgumentException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        var id = _eventService.AddEvent(validEvent).Id;
        var invalidEvent = EventsTestsHelper.GetEventDtoWithInvalidDates();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _eventService.UpdateEvent(id, invalidEvent));
    }

    [Fact]
    public void GetAll_ShouldCallGetAllOnce()
    {
        // Arrange
        var events = EventsTestsHelper.GetThreeTestEventDtos(DateTime.Now);
        var returnedEvents = new List<Event>();
        events.ForEach(e => returnedEvents.Add(e.ToEvent()));
        var emptyQuery = new EventsQuery(null, null, null, 1, events.Count);

        _mockRepository.Setup(repo => repo.GetAll()).Returns(returnedEvents);

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(emptyQuery);

        // Assert
        _mockRepository.Verify(repo => repo.GetAll(), Times.Once);
    }
}
