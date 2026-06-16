using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Application.Services;
using SupermarketPos.Infrastructure.Documents;
using SupermarketPos.Infrastructure.Persistence;

namespace SupermarketPos.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers persistence + application services. Pass a custom DB path for tests.</summary>
    public static IServiceCollection AddPos(this IServiceCollection services, string? dbPath = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        dbPath ??= DefaultDbPath();
        services.AddDbContext<PosDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        // Persistence
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IReceiptNumberGenerator, ReceiptNumberGenerator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application services
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IBarcodeService, BarcodeService>();

        // Documents (QuestPDF)
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IReportPdfService, ReportPdfService>();
        services.AddScoped<IShelfLabelService, ShelfLabelService>();

        return services;
    }

    /// <summary>Applies migrations and seeds first-run data.</summary>
    public static async Task InitializeDatabaseAsync(IServiceProvider provider, CancellationToken ct = default)
    {
        var db = provider.GetRequiredService<PosDbContext>();
        await db.Database.MigrateAsync(ct);
        var hasher = provider.GetRequiredService<IPasswordHasher>();
        await DatabaseSeeder.SeedAsync(db, hasher, ct);
    }

    public static string DefaultDbPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SupermarketPos");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "pos.db");
    }
}
