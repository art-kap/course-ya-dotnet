using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Mappings;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Infrastructure.Persistence;

namespace CourseWebApiProject.IntegrationTests;

public abstract class BaseRepositoryTests(PostgreSqlFixture fixture) : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture = fixture;

    protected string _testEventTitle = "test event";
    protected string _updatedEventTitle = "updated event";
    protected DateTime _timeNow = DateTime.Now;

    public AppDbContext CreateContext() => _fixture.CreateContext();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    protected EventCreate GetTestEventCreate()
    {
        return new EventCreate(_testEventTitle, "", _timeNow.ToUniversalTime(), _timeNow.AddHours(1).ToUniversalTime(), 5);
    }

    protected Event GetTestEvent()
    {
        return GetTestEventCreate().ToEvent();
    }

    protected EventUpdate GetTestEventUpdate()
    {
        return new EventUpdate(_updatedEventTitle, "updated description", _timeNow.AddYears(-1).ToUniversalTime(),
            _timeNow.AddYears(-1).AddHours(2).ToUniversalTime());
    }
}
