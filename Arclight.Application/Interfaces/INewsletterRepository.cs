using Arclight.Domain.Entities;

namespace Arclight.Application.Interfaces;

public interface INewsletterRepository
{
    Task<Subscriber?> GetByEmailAsync(string email);
    Task AddAsync(Subscriber subscriber);
    Task UpdateAsync(Subscriber subscriber);
    Task<List<string>> GetAllActiveEmailsAsync();
}