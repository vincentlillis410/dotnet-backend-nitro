using System.Net.Http;
using System.Text.Json;


public class CaptchaService
{
    private readonly HttpClient _http;

    public CaptchaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> VerifyAsync(string token)
    {
        var secret = Environment.GetEnvironmentVariable("CAPTCHA_SECRET_KEY");
       
        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("CAPTCHA_SECRET_KEY is not configured");

        if (string.IsNullOrWhiteSpace(token))
            return false;
        var response = await _http.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                null
            );


        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CaptchaResponse>(json);
       
      
        return result?.success == true && result.score >= 0.5;
    }
}

public class CaptchaResponse
{
    public bool success { get; set; }
    public float score { get; set; }
    
}
