using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SupermarketPos.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so <c>dotnet ef migrations</c> can build the context without the WPF app.
/// The connection string here is only used to read the model shape, not real data.
/// </summary>
public sealed class PosDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
{
    public PosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseSqlite("Data Source=pos-design.db")
            .Options;
        return new PosDbContext(options);
    }
}
