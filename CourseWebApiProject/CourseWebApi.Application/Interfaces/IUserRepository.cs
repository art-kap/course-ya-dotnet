using CourseWebApi.Domain.Entities;

namespace CourseWebApi.Application.Interfaces;

public interface IUserRepository
{
    Task Add(User user, CancellationToken ct = default);

    Task<User?> FindById(Guid userId, CancellationToken ct = default);

    Task<User?> FindByLogin(string login, CancellationToken ct = default);
}
