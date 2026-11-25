using CourseWebApi.Application.Dto;

namespace CourseWebApi.Application.Interfaces;

public interface IUserService
{
    Task AddUser(UserCreate userCreate);

    Task<string> Login(LoginRequest loginRequest);
}
