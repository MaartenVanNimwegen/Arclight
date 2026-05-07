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
        var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
        var smtpUser = _configuration["EmailSettings:Username"];
        var smtpPass = _configuration["EmailSettings:Password"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUser!, "Arclight Newsletter"),
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