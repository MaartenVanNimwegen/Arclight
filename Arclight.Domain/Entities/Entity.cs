using System;

namespace Arclight.Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTimeOffset CreatedAt { get; protected set; }
        public DateTimeOffset? UpdatedAt { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        protected Entity(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty", nameof(id));

            Id = id;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void SetUpdatedDate()
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}