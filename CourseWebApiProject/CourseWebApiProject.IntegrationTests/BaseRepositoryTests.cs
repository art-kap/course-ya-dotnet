using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Dto;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CourseWebApiProject.IntegrationTests;

public abstract class BaseRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    public virtual AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(GetTestContainerConnectionString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    protected async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE events, bookings RESTART IDENTITY CASCADE");
    }

    protected string GetTestContainerConnectionString() => _postgres.GetConnectionString();

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
