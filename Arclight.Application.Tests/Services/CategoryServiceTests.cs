using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Application.Services;
using Arclight.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arclight.Application.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<IArticleRepository> _articleRepoMock;
        private readonly Mock<ISlugService> _slugServiceMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _articleRepoMock = new Mock<IArticleRepository>();
            _slugServiceMock = new Mock<ISlugService>();

            _service = new CategoryService(
                _categoryRepoMock.Object,
                _articleRepoMock.Object,
                _slugServiceMock.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnMappedResponses()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category("Tech", "tech", "Description 1"),
                new Category("Life", "life", "Description 2")
            };
            _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _service.GetAllCategoriesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Tech");
            _categoryRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldSaveAndReturnId()
        {
            // Arrange
            var request = new CreateCategoryRequest("New Category", "Desc");
            _slugServiceMock.Setup(s => s.GenerateUniqueSlugAsync(request.Name))
                .ReturnsAsync("new-category");

            // Act
            var result = await _service.CreateCategoryAsync(request);

            // Assert
            result.Should().NotBeEmpty();
            _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenCategoryNotFound()
        {
            // Arrange
            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _service.DeleteCategoryAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowException_WhenCategoryHasArticles()
        {
            // Arrange
            var category = new Category("Test", "test", "desc");
            _categoryRepoMock.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);
            _articleRepoMock.Setup(r => r.HasArticlesInCategoryAsync(category.Id)).ReturnsAsync(true);

            // Act
            var act = () => _service.DeleteCategoryAsync(category.Id);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete category because it still contains articles.");
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange
            var category = new Category("Test", "test", "desc");
            _categoryRepoMock.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);
            _articleRepoMock.Setup(r => r.HasArticlesInCategoryAsync(category.Id)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteCategoryAsync(category.Id);

            // Assert
            result.Should().BeTrue();
            _categoryRepoMock.Verify(r => r.Delete(category), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnFalse_WhenCategoryNotFound()
        {
            // Arrange
            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _service.UpdateCategoryAsync(Guid.NewGuid(), new UpdateCategoryRequest("N", "D"));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange
            var category = new Category("Old Name", "old-slug", "Old Desc");
            var request = new UpdateCategoryRequest("New Name", "New Desc");
            _categoryRepoMock.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);

            // Act
            var result = await _service.UpdateCategoryAsync(category.Id, request);

            // Assert
            result.Should().BeTrue();
            _categoryRepoMock.Verify(r => r.Update(category), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            category.Name.Should().Be("New Name");
            category.Description.Should().Be("New Desc");
        }
    }
}