using Arclight.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arclight.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Category> Categories { get; set; }

    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Category configuration
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        // Article configuration
        modelBuilder.Entity<Article>()
            .HasIndex(a => a.Slug)
            .IsUnique();

        // Relation 1: An article has one author
        modelBuilder.Entity<Article>()
            .HasOne(a => a.Author)
            .WithMany(u => u.Articles)
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relation 2: An article has one category
        modelBuilder.Entity<Article>()
            .HasOne(a => a.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}