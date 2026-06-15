using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SupermarketPos.Infrastructure.Persistence;

namespace SupermarketPos.Tests;

/// <summary>Spins up a real SQLite in-memory database (schema from the EF model) for integration tests.</summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public PosDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseSqlite(_connection)
            .Options;
        Context = new PosDbContext(options);
        Context.Database.EnsureCreated();
    }

    public PosDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new PosDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
