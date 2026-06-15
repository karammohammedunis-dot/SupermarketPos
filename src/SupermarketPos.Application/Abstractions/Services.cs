using SupermarketPos.Application.Dtos;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Domain.Pricing;

namespace SupermarketPos.Application.Abstractions;

/// <summary>Password hashing primitive (BCrypt under the hood).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>Authentication + session. Holds the signed-in user and enforces role checks.</summary>
public interface IAuthService
{
    User? CurrentUser { get; }
    bool IsLoggedIn { get; }
    Task<User> LoginAsync(string username, string password, CancellationToken ct = default);
    void Logout();
    void RequireAdmin();
}

/// <summary>Turns a completed <see cref="Cart"/> + <see cref="Payment"/> into a persisted <see cref="Sale"/>.</summary>
public interface ISaleService
{
    /// <summary>
    /// Validates stock, decrements it, assigns a receipt number and persists the sale — all inside a
    /// single transaction. Throws <c>InsufficientStockException</c> (rolling back) if stock is short.
    /// </summary>
    Task<Sale> CheckoutAsync(Cart cart, Payment payment, User cashier, CancellationToken ct = default);
}

public interface IInventoryService
{
    Task RestockAsync(int productId, decimal quantity, CancellationToken ct = default);
    Task AdjustStockAsync(int productId, decimal newQuantity, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default);
}

public interface IReportService
{
    /// <summary>X-report: running totals for the given day, not a close.</summary>
    Task<SalesReport> GetXReportAsync(DateTime day, CancellationToken ct = default);
    /// <summary>Z-report: end-of-day close totals for the given day.</summary>
    Task<SalesReport> GetZReportAsync(DateTime day, CancellationToken ct = default);
    /// <summary>Arbitrary date-range history.</summary>
    Task<SalesReport> GetRangeReportAsync(DateTime fromInclusive, DateTime toExclusive, string title, CancellationToken ct = default);
    Task<IReadOnlyList<Sale>> GetSalesAsync(DateTime fromInclusive, DateTime toExclusive, CancellationToken ct = default);
}

public interface IReceiptService
{
    Task<byte[]> RenderPdfAsync(Sale sale, CancellationToken ct = default);
    Task SavePdfAsync(Sale sale, string path, CancellationToken ct = default);
}

/// <summary>Reports can be exported to PDF as well.</summary>
public interface IReportPdfService
{
    Task<byte[]> RenderPdfAsync(SalesReport report, CancellationToken ct = default);
    Task SavePdfAsync(SalesReport report, string path, CancellationToken ct = default);
}

/// <summary>Normalizes raw scanner / keyboard input into a clean barcode.</summary>
public interface IBarcodeService
{
    string Normalize(string raw);
}
