namespace Arclight.Application.Interfaces;

public interface INewsletterService
{
    Task<string> SubscribeAsync(string email, Guid? loggedInUserId);
    Task SendNewsletterAsync(string subject, string content);
}