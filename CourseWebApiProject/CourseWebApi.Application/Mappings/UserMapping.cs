using CourseWebApi.Application.Dto;
using CourseWebApi.Domain.Entities;

namespace CourseWebApi.Application.Mappings;

public static class UserMapping
{
    public static UserInfo ToInfo(this User user)
    {
        return new UserInfo(
            user.Id,
            user.Login);
    }
}
