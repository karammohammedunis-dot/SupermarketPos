using SupermarketPos.Domain.Common;

namespace SupermarketPos.Domain.Entities;

/// <summary>An application account (Cashier or Admin). Password is stored only as a BCrypt hash.</summary>
public sealed class User
{
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    private User() { } // EF

    public User(string username, string passwordHash, string displayName, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username cannot be empty.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");
        Username = username.Trim();
        PasswordHash = passwordHash;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? username.Trim() : displayName.Trim();
        Role = role;
    }

    public bool IsAdmin => Role == UserRole.Admin;

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");
        PasswordHash = passwordHash;
    }
}
