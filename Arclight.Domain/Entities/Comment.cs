using System;

namespace Arclight.Domain.Entities
{
    public class Comment : Entity
    {
        public string Text { get; private set; }
        public Guid ArticleId { get; private set; }
        public Guid UserId { get; private set; }

        // --- Navigation Properties ---
        public Article? Article { get; private set; }
        public User? User { get; private set; }

        public Comment(string text, Guid articleId, Guid userId) : base()
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Comment text cannot be empty");
            if (articleId == Guid.Empty) throw new ArgumentException("ArticleId is required");
            if (userId == Guid.Empty) throw new ArgumentException("UserId is required");

            Text = text;
            ArticleId = articleId;
            UserId = userId;
        }

        // Required for EF Core
        protected Comment() { }

        public void UpdateText(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText)) throw new ArgumentException("Text cannot be empty");
            Text = newText;
            SetUpdatedDate();
        }
    }
}