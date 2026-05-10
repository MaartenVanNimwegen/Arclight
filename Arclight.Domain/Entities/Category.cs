using System;
using System.Collections.Generic;

namespace Arclight.Domain.Entities
{
    public class Category : Entity
    {
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string? Description { get; private set; }

        // Navigation property (EF Core)
        // A category can have multiple articles
        public IReadOnlyCollection<Article> Articles => _articles.AsReadOnly();
        private readonly List<Article> _articles = new();

        /// <summary>
        /// Default constructor for creating new Categories
        /// </summary>
        public Category(string name, string slug, string? description = null) : base()
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug is required");

            Name = name;
            Slug = slug;
            Description = description;
        }

        // Required for ORM (EF Core)
        protected Category() { }

        // --- Domain Behaviors ---

        public void Update(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty");

            Name = name;
            Description = description;

            SetUpdatedDate();
        }
    }
}