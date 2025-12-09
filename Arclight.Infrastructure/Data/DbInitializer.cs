using Arclight.Infrastructure.Persistence;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

namespace Arclight.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Check if the database exists
            context.Database.EnsureCreated();

            // If there is dummy data already, skip
            if (context.Users.Any())
            {
                return;
            }

            // Add dummy users
            var users = new User[]
            {
                new User(Guid.NewGuid(), "peter.gerardus@gmail.com", "Peter", "Gerardus", BCrypt.Net.BCrypt.HashPassword("PeterGerardus123!"), UserRole.Admin, UserStatus.Active),
                new User(Guid.NewGuid(), "monique.degraaf@gmail.com", "Monique", "de Graaf", BCrypt.Net.BCrypt.HashPassword("MoniqueDeGraaf123!"), UserRole.Admin, UserStatus.Active),
                new User(Guid.NewGuid(), "victor.boter@gmail.com", "Viktor", "Boter", BCrypt.Net.BCrypt.HashPassword("ViktorBoter123!"), UserRole.Admin, UserStatus.Active),
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}