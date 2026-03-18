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
        // Een categorie kan meerdere artikelen hebben
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

        public void UpdateDetails(string name, string slug, string? description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty");
            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty");

            Name = name;
            Slug = slug;
            Description = description;

            SetUpdatedDate();
        }
    }
}