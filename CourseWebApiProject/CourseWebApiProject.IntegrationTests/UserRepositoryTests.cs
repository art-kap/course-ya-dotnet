using CourseWebApi.Domain.Entities;
using CourseWebApi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.IntegrationTests;

[Collection("Database collection")]
public class UserRepositoryTests(PostgreSqlFixture fixture) : BaseRepositoryTests(fixture)
{
    /// <summary>
    /// Проверяет добавление пользователя
    /// </summary>
    [Fact]
    public async Task Add_User_SavesUserToDatabase()
    {
        // Arrange
        Guid userId;
        string login, passwordHash;

        // Act
        await using (var context = CreateContext())
        {
            var user = GetTestUser();
            var userRepository = new UserRepository(context);
            await userRepository.Add(user);

            userId = user.Id;
            login = user.Login;
            passwordHash = user.PasswordHash;
        }

        User? userfromDb;

        await using (var context = CreateContext())
        {
            var userRepository = new UserRepository(context);
            userfromDb = await userRepository.FindById(userId);
        }

        // Assert
        userfromDb.Should().NotBeNull();
        userfromDb.Id.Should().Be(userId);
        userfromDb.Login.Should().Be(login);
        userfromDb.PasswordHash.Should().Be(passwordHash);
    }

    /// <summary>
    /// Проверяет добавление двух пользователей с разными логинами
    /// </summary>
    [Fact]
    public async Task AddTwoUsers_DifferentLogins_SavesUsersToDatabase()
    {
        // Arrange
        Guid firstUserId, secondUserId;

        // Act
        await using (var context = CreateContext())
        {
            var user = GetTestUser();
            var admin = GetTestAdmin();
            var userRepository = new UserRepository(context);
            await userRepository.Add(user);
            await userRepository.Add(admin);

            firstUserId = user.Id;
            secondUserId = admin.Id;
        }

        User? firstUserfromDb, secondUserfromDb;

        await using (var context = CreateContext())
        {
            var userRepository = new UserRepository(context);
            firstUserfromDb = await userRepository.FindById(firstUserId);
            secondUserfromDb = await userRepository.FindById(secondUserId);
        }

        // Assert
        firstUserfromDb.Should().NotBeNull();
        firstUserfromDb.Id.Should().Be(firstUserId);

        secondUserfromDb.Should().NotBeNull();
        secondUserfromDb.Id.Should().Be(secondUserId);
    }

    /// <summary>
    /// Проверяет добавление пользователя с уже существующим логином
    /// </summary>
    [Fact]
    public async Task AddTwoUsers_EqualLogins_ShouldThrowDbUpdateException()
    {
        // Arrange
        await using (var context = CreateContext())
        {
            var firstUser = GetTestUser();
            var userRepository = new UserRepository(context);
            await userRepository.Add(firstUser);
        }

        // Act & Assert
        await using (var context = CreateContext())
        {
            var secondUser = GetTestUser();
            var userRepository = new UserRepository(context);
            await Assert.ThrowsAsync<DbUpdateException>(() => userRepository.Add(secondUser));
        }
    }
}
