namespace AuthApi.Helpers;

public static class VerificationHelper
{
    /// <summary>
    /// Generates a 9-digit verification code.
    /// </summary>
    public static string GenerateVerificationCode()
    {
        var random = new Random();
        var code = random.Next(100000000, 999999999).ToString();
        return code;
    }
}
