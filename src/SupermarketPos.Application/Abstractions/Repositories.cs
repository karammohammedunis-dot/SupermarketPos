using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchByNameAsync(string term, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(decimal threshold, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Remove(Product product);
}

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Remove(Category category);
}

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken ct = default);
    Task<Sale?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Sale?> GetByReceiptNoAsync(string receiptNo, CancellationToken ct = default);
    Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime fromInclusive, DateTime toExclusive, CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}

public interface ISettingsRepository
{
    Task<AppSettings> GetAsync(CancellationToken ct = default);
    Task SaveAsync(AppSettings settings, CancellationToken ct = default);
}

/// <summary>Generates atomic, sequential receipt numbers (yyyy-NNNNNN).</summary>
public interface IReceiptNumberGenerator
{
    Task<string> NextAsync(DateTime now, CancellationToken ct = default);
}

/// <summary>Commits tracked changes; lets a use-case run inside a single DB transaction.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default);
}
