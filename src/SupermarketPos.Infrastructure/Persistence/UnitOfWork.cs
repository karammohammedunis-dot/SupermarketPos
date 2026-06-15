using Microsoft.EntityFrameworkCore;
using SupermarketPos.Application.Abstractions;

namespace SupermarketPos.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PosDbContext _db;
    public UnitOfWork(PosDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default)
    {
        // Use the execution strategy so the whole unit retries/rolls back as one.
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await work(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }
}
