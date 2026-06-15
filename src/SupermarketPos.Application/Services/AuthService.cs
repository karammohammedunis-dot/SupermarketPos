using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Application.Services;

/// <summary>
/// Holds the current session and authenticates against stored BCrypt hashes.
/// Registered as a singleton so the signed-in user is shared across the app.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public AuthService(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null;

    public async Task<User> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new AuthException("Username and password are required.");

        var user = await _users.GetByUsernameAsync(username.Trim(), ct);
        if (user is null || !_hasher.Verify(password, user.PasswordHash))
            throw new AuthException("Invalid username or password.");

        CurrentUser = user;
        return user;
    }

    public void Logout() => CurrentUser = null;

    public void RequireAdmin()
    {
        if (CurrentUser is null)
            throw new AuthException("Not signed in.");
        if (!CurrentUser.IsAdmin)
            throw new AuthException("Administrator privileges required.");
    }
}
