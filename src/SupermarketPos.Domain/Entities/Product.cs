using SupermarketPos.Domain.Common;

namespace SupermarketPos.Domain.Entities;

/// <summary>
/// A sellable product. <see cref="UnitPrice"/> is the KDV-inclusive shelf price (Turkish retail convention).
/// The entity owns its invariants: stock can never go negative, price/VAT/quantity must be sane.
/// </summary>
public sealed class Product
{
    public int Id { get; private set; }
    public string Barcode { get; private set; } = string.Empty;   // unique
    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }                 // KDV-inclusive shelf price (TRY)
    public decimal VatRate { get; private set; }                   // e.g. 0.01, 0.10, 0.20
    public Unit Unit { get; private set; }
    public decimal StockQuantity { get; private set; }
    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }

    private Product() { } // EF

    public Product(string barcode, string name, decimal unitPrice, decimal vatRate,
                   Unit unit, decimal stockQuantity, int categoryId)
    {
        SetBarcode(barcode);
        SetName(name);
        SetPrice(unitPrice);
        SetVatRate(vatRate);
        Unit = unit;
        if (stockQuantity < 0) throw new DomainException("Stock cannot start negative.");
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
    }

    public void DecreaseStock(decimal qty)
    {
        if (qty <= 0) throw new DomainException("Quantity must be positive.");
        if (qty > StockQuantity) throw new InsufficientStockException(Name, StockQuantity, qty);
        StockQuantity -= qty;
    }

    public void Restock(decimal qty)
    {
        if (qty <= 0) throw new DomainException("Restock quantity must be positive.");
        StockQuantity += qty;
    }

    /// <summary>Absolute stock correction used for admin adjustments (e.g. shrinkage, recount).</summary>
    public void SetStock(decimal qty)
    {
        if (qty < 0) throw new DomainException("Stock cannot be negative.");
        StockQuantity = qty;
    }

    public void SetBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new DomainException("Barcode cannot be empty.");
        Barcode = barcode.Trim();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");
        Name = name.Trim();
    }

    public void SetPrice(decimal unitPrice)
    {
        if (unitPrice < 0) throw new DomainException("Price cannot be negative.");
        UnitPrice = Money.Round2(unitPrice);
    }

    public void SetVatRate(decimal vatRate)
    {
        if (vatRate < 0 || vatRate > 1)
            throw new DomainException("VAT rate must be between 0 and 1 (e.g. 0.20 for 20%).");
        VatRate = vatRate;
    }

    public void SetUnit(Unit unit) => Unit = unit;

    public void SetCategory(int categoryId) => CategoryId = categoryId;
}
