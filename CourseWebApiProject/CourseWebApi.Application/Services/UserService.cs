using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using CourseWebApi.Domain.Entities;
using System.Security.Authentication;

namespace CourseWebApi.Application.Services;

public class UserService(IUserRepository userStore, IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IUserService
{
    private readonly IUserRepository _userStore = userStore;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    public async Task AddUser(UserCreate userCreate)
    {
        var newUser = User.Create(userCreate.Login, _passwordHasher.Hash(userCreate.Password), userCreate.Role);
        await _userStore.Add(newUser);
    }

    public async Task<string> Login(LoginRequest loginRequest)
    {
        var user = await _userStore.FindByLogin(loginRequest.Login) ?? throw new AuthenticationException();

        if (!_passwordHasher.Verify(loginRequest.Password, user.PasswordHash))
        {
            throw new AuthenticationException();
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }
}
