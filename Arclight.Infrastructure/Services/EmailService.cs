using Arclight.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Arclight.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(List<string> bccEmails, string subject, string body)
    {
        var smtpHost = _configuration["EmailSettings:Host"];
        var smtpPortRaw = _configuration["EmailSettings:Port"];
        var smtpUser = _configuration["EmailSettings:Username"];
        var smtpPass = _configuration["EmailSettings:Password"];
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@arclight.nl";
        var fromName = _configuration["EmailSettings:FromName"] ?? "Arclight Newsletter";

        if (string.IsNullOrWhiteSpace(smtpHost))
            throw new InvalidOperationException("Email configuration is missing: EmailSettings:Host is required.");
        if (string.IsNullOrWhiteSpace(smtpUser))
            throw new InvalidOperationException("Email configuration is missing: EmailSettings:Username is required.");
        if (string.IsNullOrWhiteSpace(smtpPass))
            throw new InvalidOperationException("Email configuration is missing: EmailSettings:Password is required.");
        if (string.IsNullOrWhiteSpace(smtpPortRaw))
            throw new InvalidOperationException("Email configuration is missing: EmailSettings:Port is required.");
        if (!int.TryParse(smtpPortRaw, out var smtpPort))
            throw new InvalidOperationException("Email configuration is invalid: EmailSettings:Port must be a valid integer.");

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        foreach (var email in bccEmails)
        {
            mailMessage.Bcc.Add(email);
        }

        await client.SendMailAsync(mailMessage);
    }
}