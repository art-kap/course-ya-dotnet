using CourseWebApi.Application.Interfaces;
using CourseWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApi.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task Add(User user, CancellationToken ct = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<User?> FindById(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users.FindAsync(userId, ct);
    }

    public async Task<User?> FindByLogin(string login, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Login == login, ct);
    }
}
