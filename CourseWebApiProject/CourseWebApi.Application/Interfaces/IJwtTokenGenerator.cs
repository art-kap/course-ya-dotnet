using CourseWebApi.Domain.Entities;

namespace CourseWebApi.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
