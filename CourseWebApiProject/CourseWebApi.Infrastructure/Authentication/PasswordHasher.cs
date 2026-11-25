using CourseWebApi.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace CourseWebApi.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string password, string passwordHash)
    {
        var inputHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var storedHashBytes = Convert.FromHexString(passwordHash);

        return CryptographicOperations.FixedTimeEquals(inputHashBytes, storedHashBytes);
    }
}
