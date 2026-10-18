using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.IntegrationTests;

[Collection("Database collection")]
public class MigrationTests(PostgreSqlFixture fixture) : BaseRepositoryTests(fixture)
{
    [Fact]
    public async Task ApplyMigrations_ShouldCreateDatabaseAndTablesWithoutErrors()
    {
        // Arrange
        var expectedTableNames = new List<string> { "events", "bookings" };
        var sqlCommand = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";

        // Act
        using var context = CreateContext();
        var connection = context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var canConnect = await context.Database.CanConnectAsync();

        using var command = connection.CreateCommand();
        command.CommandText = sqlCommand;
        await using var reader = await command.ExecuteReaderAsync();

        var readedTableNames = new List<string>();

        while (await reader.ReadAsync())
        {
            readedTableNames.Add(reader.GetString(0));
        }

        // Assert
        canConnect.Should().BeTrue();
        expectedTableNames.Should().AllSatisfy(name => name.Should().BeOneOf(readedTableNames));
    }
}
