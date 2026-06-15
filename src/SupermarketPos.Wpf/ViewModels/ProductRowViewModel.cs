using SupermarketPos.Domain.Entities;
using SupermarketPos.Wpf.Common;

namespace SupermarketPos.Wpf.ViewModels;

/// <summary>Read-only row in the products grid; flags low stock for highlighting.</summary>
public sealed class ProductRowViewModel
{
    public Product Product { get; }
    private readonly decimal _lowStockThreshold;

    public ProductRowViewModel(Product product, decimal lowStockThreshold)
    {
        Product = product;
        _lowStockThreshold = lowStockThreshold;
    }

    public int Id => Product.Id;
    public string Barcode => Product.Barcode;
    public string Name => Product.Name;
    public string CategoryName => Product.Category?.Name ?? "-";
    public string PriceText => MoneyText.Format(Product.UnitPrice);
    public string VatText => $"%{Product.VatRate * 100:0.#}";
    public string UnitText => Product.Unit == Domain.Common.Unit.Kilogram ? "kg" : "ad";
    public string StockText => MoneyText.Qty(Product.StockQuantity, Product.Unit == Domain.Common.Unit.Kilogram);
    public bool IsLowStock => Product.StockQuantity <= _lowStockThreshold;
}
