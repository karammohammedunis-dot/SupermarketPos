using Microsoft.EntityFrameworkCore;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Infrastructure.Persistence;

public sealed class ProductRepository : IProductRepository
{
    private readonly PosDbContext _db;
    public ProductRepository(PosDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default) =>
        _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Barcode == barcode, ct);

    public async Task<Product?> GetByPluAsync(string plu, CancellationToken ct = default)
    {
        // PLU lives in digits 3-7 of a 13-digit '2'-prefixed barcode. Match in memory (small catalog).
        var candidates = await _db.Products.Include(p => p.Category).ToListAsync(ct);
        return candidates.FirstOrDefault(p =>
            p.Barcode.Length == 13 && p.Barcode[0] == '2' && p.Barcode.Substring(2, 5) == plu);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string term, CancellationToken ct = default)
    {
        term = (term ?? string.Empty).Trim();
        return await _db.Products.Include(p => p.Category)
            .Where(p => EF.Functions.Like(p.Name, $"%{term}%") || p.Barcode.StartsWith(term))
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default) =>
        await _db.Products.Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId).OrderBy(p => p.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(decimal threshold, CancellationToken ct = default)
    {
        // Money/quantity are TEXT in SQLite, so filter the numeric comparison in memory.
        var all = await _db.Products.Include(p => p.Category).ToListAsync(ct);
        return all.Where(p => p.StockQuantity <= threshold)
                  .OrderBy(p => p.StockQuantity).ToList();
    }

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await _db.Products.AddAsync(product, ct);

    public void Remove(Product product) => _db.Products.Remove(product);
}

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly PosDbContext _db;
    public CategoryRepository(PosDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Categories.OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await _db.Categories.AddAsync(category, ct);

    public void Remove(Category category) => _db.Categories.Remove(category);
}

public sealed class SaleRepository : ISaleRepository
{
    private readonly PosDbContext _db;
    public SaleRepository(PosDbContext db) => _db = db;

    public async Task AddAsync(Sale sale, CancellationToken ct = default) =>
        await _db.Sales.AddAsync(sale, ct);

    public Task<Sale?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Sales.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Sale?> GetByReceiptNoAsync(string receiptNo, CancellationToken ct = default) =>
        _db.Sales.Include(s => s.Lines).FirstOrDefaultAsync(s => s.ReceiptNo == receiptNo, ct);

    public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime fromInclusive, DateTime toExclusive, CancellationToken ct = default) =>
        await _db.Sales.Include(s => s.Lines)
            .Where(s => s.CreatedAt >= fromInclusive && s.CreatedAt < toExclusive)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
}

public sealed class UserRepository : IUserRepository
{
    private readonly PosDbContext _db;
    public UserRepository(PosDbContext db) => _db = db;

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Users.OrderBy(u => u.Username).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _db.Users.AddAsync(user, ct);
}

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly PosDbContext _db;
    public SettingsRepository(PosDbContext db) => _db = db;

    public async Task<AppSettings> GetAsync(CancellationToken ct = default)
    {
        var settings = await _db.Settings.FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = new AppSettings("Süpermarket", string.Empty, "tr", 5m, new[] { 0.01m, 0.10m, 0.20m });
            await _db.Settings.AddAsync(settings, ct);
            await _db.SaveChangesAsync(ct);
        }
        return settings;
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
        if (settings.Id == 0)
            await _db.Settings.AddAsync(settings, ct);
        else
            _db.Settings.Update(settings);
        await _db.SaveChangesAsync(ct);
    }
}
