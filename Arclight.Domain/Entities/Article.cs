using System;

namespace Arclight.Domain.Entities
{
    public class Article : Entity
    {
        public string Title { get; private set; }
        public string Slug { get; private set; }
        public string Summary { get; private set; }
        public string Content { get; private set; }
        public bool IsPublished { get; private set; }
        public DateTimeOffset? PublishedAt { get; private set; }

        // --- Foreign Keys ---
        public Guid AuthorId { get; private set; }
        public Guid CategoryId { get; private set; }

        // --- Navigation Properties (EF Core) ---
        public User? Author { get; private set; }
        public Category? Category { get; private set; }

        /// <summary>
        /// Default constructor for creating a new Article
        /// </summary>
        public Article(
            string title,
            string slug,
            string summary,
            string content,
            Guid authorId,
            Guid categoryId) : base()
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug is required");
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required");
            if (authorId == Guid.Empty) throw new ArgumentException("AuthorId cannot be empty");
            if (categoryId == Guid.Empty) throw new ArgumentException("CategoryId cannot be empty");

            Title = title;
            Slug = slug;
            Summary = summary;
            Content = content;
            AuthorId = authorId;
            CategoryId = categoryId;

            IsPublished = false; // Always start as a draft
        }

        // Required for ORM (EF Core)
        protected Article() { }

        // --- Domain Behaviors ---

        public void UpdateContent(string title, string slug, string summary, string content)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty");
            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty");
            if (string.IsNullOrWhiteSpace(summary)) throw new ArgumentException("Summary cannot be empty");
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be empty");

            Title = title;
            Slug = slug;
            Summary = summary;
            Content = content;

            SetUpdatedDate();
        }

        public void ChangeCategory(Guid newCategoryId)
        {
            if (newCategoryId == Guid.Empty) throw new ArgumentException("CategoryId cannot be empty");

            if (CategoryId == newCategoryId) return;

            CategoryId = newCategoryId;
            SetUpdatedDate();
        }

        public void Publish()
        {
            if (IsPublished) return;

            IsPublished = true;
            PublishedAt = DateTimeOffset.UtcNow;
            SetUpdatedDate();
        }

        public void Unpublish()
        {
            if (!IsPublished) return;

            IsPublished = false;
            PublishedAt = null;
            SetUpdatedDate();
        }
    }
}