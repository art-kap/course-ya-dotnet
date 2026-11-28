using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using CourseWebApi.Application.Services;
using CourseWebApi.Domain.Exceptions;
using CourseWebApi.Infrastructure.Persistence;
using CourseWebApi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CourseWebApiProject.Tests;

public class UserServiceTests
{
    private readonly ServiceProvider _serviceProvider;

    public UserServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped(_ => Mock.Of<IPasswordHasher>(p => p.Hash(It.IsAny<string>()) == string.Empty));
        services.AddScoped(_ => Mock.Of<IJwtTokenGenerator>());

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Успешное добавление пользователя
    /// </summary>
    [Fact]
    public async Task Add_ValidUser_Success()
    {
        // Arrange
        var validUser = EventsTestsHelper.GetTestUserCreate();

        // Act
        UserInfo response;

        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            response = await userService.AddUser(validUser);
        }

        // Assert
        response.Should().NotBeNull();
        response.Login.Should().BeEquivalentTo(validUser.Login);
    }

    /// <summary>
    /// Проверяет валидацию логина и пароля при создании пользователя
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData("", "pass")]
    [InlineData("    ", "pass")]
    [InlineData("login", "")]
    public async Task Add_InvalidUser_ShouldThrowUserValidationException(string login, string password)
    {
        // Arrange
        var invalidUser = EventsTestsHelper.GetTestUserCreate(login, password);

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            await Assert.ThrowsAsync<UserValidationException>(() => userService.AddUser(invalidUser));
        }
    }
}
