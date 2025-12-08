using Arclight.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}