using Microsoft.EntityFrameworkCore;
using SupermarketPos.Application.Abstractions;

namespace SupermarketPos.Infrastructure.Persistence;

/// <summary>
/// Produces sequential receipt numbers per year in the form <c>yyyy-NNNNNN</c>. Because the
/// suffix is zero-padded to a fixed width, ordering by the string yields the latest number.
/// Called inside the checkout transaction so concurrent sales cannot collide.
/// </summary>
public sealed class ReceiptNumberGenerator : IReceiptNumberGenerator
{
    private readonly PosDbContext _db;
    public ReceiptNumberGenerator(PosDbContext db) => _db = db;

    public async Task<string> NextAsync(DateTime now, CancellationToken ct = default)
    {
        var prefix = $"{now.Year}-";
        var last = await _db.Sales
            .Where(s => s.ReceiptNo.StartsWith(prefix))
            .OrderByDescending(s => s.ReceiptNo)
            .Select(s => s.ReceiptNo)
            .FirstOrDefaultAsync(ct);

        var next = 1;
        if (last is not null && int.TryParse(last.Substring(prefix.Length), out var n))
            next = n + 1;

        return prefix + next.ToString("D6");
    }
}
