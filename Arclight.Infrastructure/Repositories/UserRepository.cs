using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arclight.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task AddAsync(User user) => await context.Users.AddAsync(user);

        public async Task<User?> GetByEmailAsync(string email) => await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByIdAsync(Guid id) => await context.Users.FindAsync(id);

        public async Task SaveChangesAsync() => await context.SaveChangesAsync();

        public async Task<IEnumerable<User>> GetAllUsersAsync() => await context.Users.ToListAsync();

        public async Task UpdateUserRoleAsync(Guid id, UserRole role)
        {
            User? user = await context.Users.FindAsync(id);
            if (user is null)
            {
                throw new KeyNotFoundException("User not found");
            }
                
            user.ChangeRole(role);
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }

        public void Delete(User user)
        {
            context.Users.Remove(user);
        }
    }
}
