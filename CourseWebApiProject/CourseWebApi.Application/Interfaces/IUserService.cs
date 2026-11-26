using CourseWebApi.Application.Dto;

namespace CourseWebApi.Application.Interfaces;

public interface IUserService
{
    Task<UserInfo> AddUser(UserCreate userCreate);

    Task<string> Login(LoginRequest loginRequest);
}
