using FluentAssertions;
using SupermarketPos.Application.Services;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Infrastructure.Persistence;
using Xunit;

namespace SupermarketPos.Tests;

public class AuthServiceTests
{
    private static AuthService Build(TestDb db, out BCryptPasswordHasher hasher)
    {
        hasher = new BCryptPasswordHasher();
        db.Context.Users.Add(new User("admin", hasher.Hash("admin"), "Yönetici", UserRole.Admin));
        db.Context.Users.Add(new User("kasa", hasher.Hash("1234"), "Kasiyer", UserRole.Cashier));
        db.Context.SaveChanges();
        return new AuthService(new UserRepository(db.Context), hasher);
    }

    [Fact]
    public void Hasher_VerifiesCorrectPassword()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("secret");
        hasher.Verify("secret", hash).Should().BeTrue();
        hasher.Verify("wrong", hash).Should().BeFalse();
    }

    [Fact]
    public async Task Login_Succeeds_WithCorrectCredentials()
    {
        using var db = new TestDb();
        var auth = Build(db, out _);

        var user = await auth.LoginAsync("admin", "admin");

        user.Username.Should().Be("admin");
        auth.IsLoggedIn.Should().BeTrue();
        auth.CurrentUser!.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task Login_Fails_WithWrongPassword()
    {
        using var db = new TestDb();
        var auth = Build(db, out _);

        var act = async () => await auth.LoginAsync("admin", "nope");

        await act.Should().ThrowAsync<AuthException>();
        auth.IsLoggedIn.Should().BeFalse();
    }

    [Fact]
    public async Task RequireAdmin_BlocksCashier()
    {
        using var db = new TestDb();
        var auth = Build(db, out _);
        await auth.LoginAsync("kasa", "1234");

        var act = () => auth.RequireAdmin();

        act.Should().Throw<AuthException>();
    }

    [Fact]
    public async Task RequireAdmin_AllowsAdmin()
    {
        using var db = new TestDb();
        var auth = Build(db, out _);
        await auth.LoginAsync("admin", "admin");

        var act = () => auth.RequireAdmin();

        act.Should().NotThrow();
    }
}
