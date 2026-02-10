using Lms.Api.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Lms.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var emailSettings = _config.GetSection("Email");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            emailSettings["FromName"] ?? "LMS Platform",
            emailSettings["FromEmail"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Reset Your Password - LMS Platform";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Click the link below:</p>
                <p><a href="{resetLink}">Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
                """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            emailSettings["SmtpHost"],
            int.Parse(emailSettings["SmtpPort"] ?? "587"),
            MailKit.Security.SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            emailSettings["SmtpUser"],
            emailSettings["SmtpPass"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }
}
