using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Arclight.Infrastructure.Repositories;

public class NewsletterRepository : INewsletterRepository
{
    private readonly AppDbContext _context;

    private const string PostgresUniqueViolationCode = "23505";

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
    }

    public Task UpdateAsync(Subscriber subscriber)
    {
        _context.Subscribers.Update(subscriber);
        return Task.CompletedTask;
    }

    public Task<List<string>> GetAllActiveEmailsAsync()
    {
        return _context.Subscribers
            .Where(s => s.IsActive)
            .Select(s => s.Email)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresUniqueViolationCode)
        {
            throw new InvalidOperationException("This email address is already subscribed.", ex);
        }
    }
}