using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Mappings;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Infrastructure.Persistence;

namespace CourseWebApiProject.IntegrationTests;

public abstract class BaseRepositoryTests(PostgreSqlFixture fixture) : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture = fixture;

    public AppDbContext CreateContext() => _fixture.CreateContext();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    protected EventCreate GetTestEventCreate()
    {
        return new EventCreate("test event", "", DateTime.Now.ToUniversalTime(),
            DateTime.Now.AddHours(1).ToUniversalTime(), 5);
    }

    protected Event GetTestEvent()
    {
        return GetTestEventCreate().ToEvent();
    }

    protected EventUpdate GetTestEventUpdate()
    {
        return new EventUpdate("updated event", "updated description", DateTime.Now.AddYears(-1).ToUniversalTime(),
            DateTime.Now.AddYears(-1).AddHours(2).ToUniversalTime());
    }
}
