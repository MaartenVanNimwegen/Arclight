using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Ensure that the database is up-to-date
            context.Database.Migrate();

            // 1. Seed Users if not already seeded
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User(Guid.NewGuid(), "peter.gerardus@gmail.com", "Peter", "Gerardus", BCrypt.Net.BCrypt.HashPassword("PeterGerardus123!"), UserRole.Admin, UserStatus.Active),
                    new User(Guid.NewGuid(), "monique.degraaf@gmail.com", "Monique", "de Graaf", BCrypt.Net.BCrypt.HashPassword("MoniqueDeGraaf123!"), UserRole.ContentCreator, UserStatus.Active),
                    new User(Guid.NewGuid(), "dieter.gieter@gmail.com", "Dieter", "Gieter", BCrypt.Net.BCrypt.HashPassword("DieterGieter123!"), UserRole.User, UserStatus.Active)
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // 2. Seed categories if not already seeded
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
            new Category("Technologie", "technologie", "Alles over software en hardware."),
            new Category("Marketing", "marketing", "Content marketing en SEO tips.")
        };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // 3. Seed articles if not already seeded
            if (!context.Articles.Any())
            {
                // Get existing users and categories
                var author = context.Users.First();
                var category = context.Categories.First(c => c.Slug == "technologie");

                var article = new Article(
                    "Welkom bij Arclight",
                    "welkom-bij-arclight",
                    "Dit is de introductie tot het nieuwe blog platform.",
                    "Content marketing is ontzettend belangrijk in het huidige digitale landschap. Arclight lost dit op door een modern platform te bieden...",
                    author.Id,
                    category.Id);

                article.Publish();

                context.Articles.Add(article);
                context.SaveChanges();
            }
        }
    }
}
