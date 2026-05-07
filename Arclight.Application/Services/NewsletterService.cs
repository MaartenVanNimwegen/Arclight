using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;

namespace Arclight.Application.Services;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterRepository _repository;
    private readonly IEmailService _emailService;

    public NewsletterService(INewsletterRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task<string> SubscribeAsync(string email, Guid? loggedInUserId)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Ongeldig e-mailadres.");

        var emailToSave = email.ToLowerInvariant().Trim();

        var existingSubscriber = await _repository.GetByEmailAsync(emailToSave);

        if (existingSubscriber != null)
        {
            if (!existingSubscriber.IsActive)
            {
                existingSubscriber.Resubscribe();

                if (loggedInUserId.HasValue)
                {
                    existingSubscriber.LinkToUser(loggedInUserId.Value);
                }

                await _repository.UpdateAsync(existingSubscriber);
                return "Welkom terug! Je bent weer ingeschreven.";
            }

            throw new InvalidOperationException("Dit e-mailadres is al ingeschreven.");
        }

        Subscriber newSubscriber = loggedInUserId.HasValue
            ? new Subscriber(emailToSave, loggedInUserId.Value)
            : new Subscriber(emailToSave);

        await _repository.AddAsync(newSubscriber);

        return "Bedankt voor je inschrijving!";
    }

    public async Task SendNewsletterAsync(string subject, string content)
    {
        var emails = await _repository.GetAllActiveEmailsAsync();

        if (emails == null || emails.Count == 0)
            throw new InvalidOperationException("Er zijn geen actieve abonnees om de nieuwsbrief naar te verzenden.");

        await _emailService.SendEmailAsync(emails, subject, content);
    }
}