using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string username, string verificationCode)
    {
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var smtpHost = _configuration["EmailSettings:SmtpHost"];

        Console.WriteLine($"SMTP Host: {smtpHost}");
        var smtpPortString = _configuration["EmailSettings:SmtpPort"];
        var smtpPort = int.Parse(smtpPortString ?? "587");
        var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Mail", fromEmail));
        message.To.Add(new MailboxAddress(username, toEmail));
        message.Subject = "Verify your email";

       var bodyBuilder = new BodyBuilder
{
    HtmlBody = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8' />
            <meta name='viewport' content='width=device-width, initial-scale=1.0' />
            <title>Email Verification</title>
            <style>
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background-color: #f4f6f9;
                    color: #333;
                    padding: 20px;
                    margin: 0;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                }}
                .email-container {{
                    max-width: 600px;
                    width: 100%;
                    background-color: white;
                    padding: 30px;
                    border-radius: 8px;
                    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
                }}
                h2 {{
                    color: #2c3e50;
                    font-size: 24px;
                    margin-bottom: 10px;
                }}
                p {{
                    font-size: 16px;
                    line-height: 1.6;
                }}
                .code-container {{
                    display: flex;
                    justify-content: center;
                    margin: 20px 0;
                }}
                .verification-code {{
                    background: #3498db;
                    color: white;
                    padding: 12px 24px;
                    border-radius: 25px;
                    font-size: 20px;
                    font-weight: bold;
                    letter-spacing: 1px;
                }}
                footer {{
                    margin-top: 30px;
                    text-align: center;
                    font-size: 14px;
                    color: #95a5a6;
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <h2>Email Verification</h2>
                <p>Hello <strong>{username}</strong>,</p>
                <p>Thank you for registering with us! Please verify your email address by copying the code below:</p>
                <div class='code-container'>
                    <h3 class='verification-code'>{verificationCode}</h3>
                </div>
                <p>This verification code is valid for the next 15 minutes and can only be used once.</p>
                <p>If you didn't sign up for this account, feel free to disregard this email.</p>
                <footer>
                    <p>© 2026 Nitro. All rights reserved.</p>
                </footer>
            </div>
        </body>
        </html>",
    TextBody = $"Hello {username},\n\nThank you for registering! Please verify your email by entering this code: {verificationCode}\n\nThis code expires in 15 minutes."
};


        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the signup
            Console.WriteLine($"Error sending verification email: {ex.Message}");
        }
    }
}
public class VerificationHelper
{
    private static Random _random = new Random();

    public static string GenerateVerificationCode()
    {
        return _random.Next(100000000, 999999999).ToString();
    }
}
