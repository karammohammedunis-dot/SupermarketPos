using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Domain.Pricing;

/// <summary>
/// A line in an in-progress sale: a product, a quantity, and an optional per-line discount.
/// All amounts are KDV-inclusive (gross), matching the shelf price.
/// </summary>
public sealed class CartLine
{
    public Product Product { get; }
    public decimal Quantity { get; private set; }
    public Discount? LineDiscount { get; private set; }

    public CartLine(Product product, decimal quantity)
    {
        Product = product ?? throw new DomainException("Product is required.");
        SetQuantity(quantity);
    }

    public decimal GrossUnitPrice => Product.UnitPrice;                           // KDV-inclusive
    public decimal GrossSubtotal => Money.Round2(GrossUnitPrice * Quantity);      // before line discount
    public decimal GrossTotal => LineDiscount?.Apply(GrossSubtotal) ?? GrossSubtotal;
    public decimal VatRate => Product.VatRate;

    public void SetQuantity(decimal quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");
        if (Product.Unit == Unit.Piece && quantity != Math.Truncate(quantity))
            throw new DomainException("Piece items must have a whole-number quantity.");
        Quantity = quantity;
    }

    public void Increment(decimal by)
    {
        if (by <= 0) throw new DomainException("Increment must be positive.");
        SetQuantity(Quantity + by);
    }

    public void ApplyDiscount(Discount? discount) => LineDiscount = discount;
}
