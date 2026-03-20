using Arclight.Domain.Entities;

namespace Arclight.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
}
