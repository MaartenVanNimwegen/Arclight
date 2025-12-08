using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using System;

namespace Arclight.Domain.Entities;

public class User : Entity
{
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTimeOffset? LastLoggedinDate { get; private set; }
    public string FullName => $"{FirstName}, {LastName}";

    /// <summary>
    /// Default constructor for creating new Users
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="passwordHash"></param>
    /// <param name="role"></param>
    /// <exception cref="ArgumentException"></exception>
    public User(
        string email,
        string firstName,
        string lastName,
        string passwordHash,
        UserRole role) : base()
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required");

        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.Active; // Default to Active on creation
    }

    /// <summary>
    /// Constructor for loading existing data or Seeding (allows specifying ID)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="passwordHash"></param>
    /// <param name="role"></param>
    /// <param name="status"></param>
    public User(
        Guid id,
        string email,
        string firstName,
        string lastName,
        string passwordHash,
        UserRole role,
        UserStatus status) : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Role = role;
        Status = status;
    }

    // Required for ORM (EF Core) to instantiate the object without parameters
    protected User() { }

    // --- Domain Behaviors ---

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be empty");

        FirstName = firstName;
        LastName = lastName;
        SetUpdatedDate();
    }

    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail)) throw new ArgumentException("Email cannot be empty");

        Email = newEmail;
        SetUpdatedDate();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Password hash cannot be empty");

        PasswordHash = newPasswordHash;
        SetUpdatedDate();
    }

    public void ChangeRole(UserRole newRole)
    {
        if (Role == newRole) return;

        Role = newRole;
        SetUpdatedDate();
    }

    public void RecordLogin()
    {
        LastLoggedinDate = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (Status == UserStatus.Active) return;

        Status = UserStatus.Active;
        SetUpdatedDate();
    }

    public void Deactivate()
    {
        if (Status == UserStatus.Inactive) return;

        Status = UserStatus.Inactive;
        SetUpdatedDate();
    }
}