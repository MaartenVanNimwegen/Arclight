using Arclight.Application.DTOs;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

namespace Arclight.Application.Interfaces
{
    public interface IUserService
    {
        Task<Guid> CreateUserAsync(string email, string firstName, string lastName, string password, UserRole role);
        Task<User?> GetUserAsync(Guid id);
        Task<string?> LoginAsync(LoginRequest request);
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task UpdateUserRoleAsync(Guid id, UserRole role);
        Task DeleteUserAsync(Guid id);
    }
}
