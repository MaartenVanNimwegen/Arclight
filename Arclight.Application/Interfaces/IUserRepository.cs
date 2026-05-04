using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arclight.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserRoleAsync(Guid id, UserRole role);
        void Delete(User user);
    }
}
