namespace CourseWebApi.Domain.Entities;

public class User
{
    private User()
    {
    }

    private User(string login, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    public Guid Id { get; private set; }
    public string Login { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }

    public static User Create(string login, string passwordHash, UserRole role)
    {
        return new User(login, passwordHash, role);
    }
}
