using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AuthApi.Services;

public static class PasswordService
{
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        var hash = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            )
        );

        return $"{Convert.ToBase64String(salt)}.{hash}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        var salt = Convert.FromBase64String(parts[0]);
        var hash = parts[1];

        var inputHash = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                100000,
                32
            )
        );

        return hash == inputHash;
    }
}
