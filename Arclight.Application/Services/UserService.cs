using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arclight.Application.Services
{
    public interface IUserService
    {
        Task<Guid> CreateUserAsync(string email, string firstName, string lastName, string passwordHash, UserRole role);
        Task<User?> GetUserAsync(Guid id);
    }

    public class UserService(IUserRepository repository) : IUserService
    {
        public async Task<Guid> CreateUserAsync(string email, string firstName, string lastName, string passwordHash, UserRole role)
        {
            var user = new User(email, firstName, lastName, passwordHash, role);
            await repository.AddAsync(user);
            await repository.SaveChangesAsync();
            return user.Id;
        }

        public async Task<User?> GetUserAsync(Guid id)
        {
            return await repository.GetByIdAsync(id);
        }
    }
}
