using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Application.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly IProductRepository _products;
    private readonly ISettingsRepository _settings;
    private readonly IUnitOfWork _uow;

    public InventoryService(IProductRepository products, ISettingsRepository settings, IUnitOfWork uow)
    {
        _products = products;
        _settings = settings;
        _uow = uow;
    }

    public async Task RestockAsync(int productId, decimal quantity, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
                      ?? throw new DomainException("Product not found.");
        product.Restock(quantity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task AdjustStockAsync(int productId, decimal newQuantity, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
                      ?? throw new DomainException("Product not found.");
        product.SetStock(newQuantity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        return await _products.GetLowStockAsync(settings.LowStockThreshold, ct);
    }
}
