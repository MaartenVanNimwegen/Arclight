using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Repositories;

public class NewsletterRepository : INewsletterRepository
{
    private readonly AppDbContext _context;

    public NewsletterRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Subscriber?> GetByEmailAsync(string email)
    {
        return await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task AddAsync(Subscriber subscriber)
    {
        await _context.Subscribers.AddAsync(subscriber);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subscriber subscriber)
    {
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();
    }

    public Task<List<string>> GetAllActiveEmailsAsync()
    {
        return _context.Subscribers
            .Where(s => s.IsActive)
            .Select(s => s.Email)
            .ToListAsync();
    }
}