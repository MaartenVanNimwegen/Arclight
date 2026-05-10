namespace Arclight.Domain.Entities;

public class Subscriber : Entity
{
    public string Email { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset SubscribedAt { get; private set; }
    public DateTimeOffset? UnsubscribedAt { get; private set; }

    public Guid? UserId { get; private set; }

    // Navigation property for EF Core
    public User? User { get; private set; }

    /// <summary>
    /// Constructor for anonymous subscribers (without account)
    /// </summary>
    public Subscriber(string email) : base()
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email address is required.");
        if (!email.Contains('@')) throw new ArgumentException("Invalid email address format.");

        Email = email.ToLowerInvariant().Trim();
        IsActive = true;
        SubscribedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Constructor for subscribers linked to a user account.
    /// </summary>
    public Subscriber(string email, Guid userId) : this(email)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId must not be empty.");

        UserId = userId;
    }

    protected Subscriber() { }

    // --- Domain Behaviors ---

    /// <summary>
    /// Changes the email address of the subscriber.
    /// </summary>
    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail)) throw new ArgumentException("Email address must not be empty.");
        if (!newEmail.Contains('@')) throw new ArgumentException("Invalid email address format.");

        var formattedEmail = newEmail.ToLowerInvariant().Trim();

        if (Email == formattedEmail) return;

        Email = formattedEmail;

        SetUpdatedDate();
    }

    /// <summary>
    /// Unsubscribes the subscriber, marking them as inactive and recording the unsubscribed date.
    /// </summary>
    public void Unsubscribe()
    {
        if (!IsActive) return;

        IsActive = false;
        UnsubscribedAt = DateTimeOffset.UtcNow;

        SetUpdatedDate();
    }

    /// <summary>
    /// Resubscribes the subscriber, marking them as active and clearing the unsubscribed date.
    /// </summary>
    public void Resubscribe()
    {
        if (IsActive) return;

        IsActive = true;
        UnsubscribedAt = null;

        SetUpdatedDate();
    }

    /// <summary>
    /// Links the subscriber to a user account by setting the UserId.
    /// Used when a logged-in user subscribes with an email that wasn't previously linked to their account,
    /// or when a user logs in while already having a subscription with that email address.
    /// </summary>
    public void LinkToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must not be empty.");

        if (UserId == userId) return;

        UserId = userId;

        SetUpdatedDate();
    }
}
