using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Infrastructure.Persistence;
using Arclight.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Arclight.Infrastructure.Tests.Repositories;

public class ArticleRepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllPublishedAsync_ShouldReturnOnlyPublishedArticles_OrderedByDate()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new ArticleRepository(context);

        var author = new User("author@test.nl", "John", "Doe", "hash", UserRole.ContentCreator);
        var category = new Category("Tech", "tech", "desc");

        var articleOld = new Article("Oud", "old", "sum", "cont", author.Id, category.Id);
        articleOld.Publish();

        var publishedAtProp = typeof(Article).GetProperty(nameof(Article.PublishedAt));
        publishedAtProp?.SetValue(articleOld, (DateTimeOffset?)DateTimeOffset.UtcNow.AddDays(-1));

        var articleNew = new Article("Nieuw", "new", "sum", "cont", author.Id, category.Id);
        articleNew.Publish();
        publishedAtProp?.SetValue(articleNew, (DateTimeOffset?)DateTimeOffset.UtcNow);

        var articleDraft = new Article("Draft", "draft", "sum", "cont", author.Id, category.Id);

        context.Users.Add(author);
        context.Categories.Add(category);
        context.Articles.AddRange(articleOld, articleNew, articleDraft);
        await context.SaveChangesAsync();

        // Act
        var result = (await repo.GetAllPublishedAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Nieuw");
        result.Should().NotContain(a => a.Title == "Draft");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnArticle_WithIncludes()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new ArticleRepository(context);

        var author = new User("author@test.nl", "John", "Doe", "hash", UserRole.ContentCreator);
        var category = new Category("Tech", "tech", "desc");
        var article = new Article("Target", "target-slug", "sum", "cont", author.Id, category.Id);

        context.Users.Add(author);
        context.Categories.Add(category);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetBySlugAsync("target-slug");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Target");
        result.Author.Should().NotBeNull();
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldReturnTrue_WhenSlugExists()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new ArticleRepository(context);
        var article = new Article("T", "exists", "S", "C", Guid.NewGuid(), Guid.NewGuid());

        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Act & Assert
        (await repo.SlugExistsAsync("exists")).Should().BeTrue();
        (await repo.SlugExistsAsync("does-not-exist")).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ShouldRemoveArticleFromContext()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new ArticleRepository(context);
        var article = new Article("T", "del", "S", "C", Guid.NewGuid(), Guid.NewGuid());
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Act
        repo.Delete(article);
        await repo.SaveChangesAsync();

        // Assert
        context.Articles.Should().BeEmpty();
    }
}