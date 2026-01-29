using AuthApi.Models;
using AuthApi.Services;
using AuthApi.Helpers;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtService _jwtService;
    private readonly CaptchaService _captchaService;
    private readonly EmailService _emailService;

    public AuthController(UserService userService, JwtService jwtService, CaptchaService captchaService, EmailService emailService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _captchaService = captchaService;
        _emailService = emailService;
    }

    // -------------------------
    // SIGNUP (EMAIL/PASSWORD)
    // -------------------------
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] UserDto dto)
    {
        var captchaValid = await _captchaService.VerifyAsync(dto.recaptchaToken);
        if (!captchaValid)
            return BadRequest("CAPTCHA verification failed");

        var existing = await _userService.GetByEmailAsync(dto.Email);
        if (existing != null)
            return BadRequest("User already exists");

        var verificationCode = VerificationHelper.GenerateVerificationCode();
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = PasswordService.HashPassword(dto.Password),
            Provider = "local",
            EmailVerified = false,
            VerificationToken = Guid.NewGuid().ToString("N"),
            VerificationTokenExpires = DateTime.UtcNow.AddHours(24),
           EmailVerificationCode = verificationCode
        };
         // Generate a 9-digit code

        // Save the verification code in the user's record (you can use a field like EmailVerificationCode)

        
        await _userService.UpdateAsync(user);
        await _emailService.SendVerificationEmailAsync(user.Email, user.Name, verificationCode);
Console.WriteLine(user);
        await _userService.CreateAsync(user);

        return Ok(new { message = "User created. Please check your email to verify your account." });
    }

 // POST: Verify the code entered by the user
    [HttpPost("verify-number")]
    public async Task<IActionResult> VerifyEmail(VerifyCodeDto dto)
    {
        var user = await _userService.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return BadRequest("User not found.");
        }
        Console.WriteLine(dto.Email);
        Console.WriteLine(dto.Code);
        Console.WriteLine(user.EmailVerificationCode);
        // Check if the code matches
        if (user.EmailVerificationCode == dto.Code)
        {
            // Verification successful, mark the user as verified
            user.EmailVerified = true;
            await _userService.UpdateAsync(user);
            return Ok("Email verified successfully.");
        }

        return BadRequest("Invalid verification code.");
    }
    // -------------------------
    // EMAIL VERIFICATION
    // -------------------------
    [HttpGet("verify")]
    public async Task<IActionResult> Verify([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token missing");

        var user = await _userService.GetByVerificationTokenAsync(token);
        if (user == null)
            return BadRequest("Invalid token");

        if (user.VerificationTokenExpires == null || user.VerificationTokenExpires < DateTime.UtcNow)
            return BadRequest("Token expired");

        user.EmailVerified = true;
        user.VerificationToken = null;
        user.VerificationTokenExpires = null;
        await _userService.UpdateAsync(user);

        return Ok(new { message = "Email verified" });
    }

    // -------------------------
    // LOGIN (EMAIL/PASSWORD)
    // -------------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Verify CAPTCHA first

        
        var user = await _userService.GetByEmailAsync(dto.Email);
        Console.WriteLine(user);
        if (user == null || user.Provider != "local")
            return Unauthorized("Invalid credentials");

        if (!PasswordService.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");
/*
        if (!user.EmailVerified)
            return Unauthorized("Email not verified");
*/
        var token = _jwtService.GenerateToken(user.Email);

        return Ok(new
        {
            token,
            username = user.Name,
            email = user.Email
        });
    }

    // -------------------------
    // GOOGLE LOGIN
    // -------------------------
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IdToken))
            return BadRequest("Google token missing");

        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
        }
        catch
        {
            return Unauthorized("Invalid Google token");
        }

        var email = payload.Email;
        var name = payload.Name ?? payload.GivenName ?? "Google User";

        var user = await _userService.GetByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = string.Empty,
                Provider = "google"
            };

            await _userService.CreateAsync(user);
        }

        var token = _jwtService.GenerateToken(user.Email);

        return Ok(new
        {
            token,
            username = user.Name,
            email = user.Email
        });
    }
}

// DTOs
public record UserDto(string Name, string Email, string Password, string recaptchaToken);
public record VerifyCodeDto(string Email, string Code);
public record LoginDto(string Email, string Password);
public record GoogleLoginDto(string IdToken);

