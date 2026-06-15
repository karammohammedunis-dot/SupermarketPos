using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Domain.Pricing;

/// <summary>
/// The live sale aggregate the cashier builds on the register. Holds lines and an optional
/// whole-cart discount. Totals are intentionally NOT computed here — all money math lives in
/// <see cref="PricingService"/> so there is a single source of truth.
/// </summary>
public sealed class Cart
{
    private readonly List<CartLine> _lines = new();
    public IReadOnlyList<CartLine> Lines => _lines;
    public Discount? CartDiscount { get; private set; }

    public bool IsEmpty => _lines.Count == 0;
    public decimal ItemCount => _lines.Sum(l => l.Product.Unit == Unit.Piece ? l.Quantity : 1m);

    /// <summary>Add a product, merging into an existing line for the same product.</summary>
    public CartLine AddOrIncrement(Product product, decimal quantity)
    {
        if (product is null) throw new DomainException("Product is required.");
        var existing = _lines.FirstOrDefault(l => l.Product.Id == product.Id && product.Id != 0)
                       ?? _lines.FirstOrDefault(l => ReferenceEquals(l.Product, product));
        if (existing is not null)
        {
            existing.Increment(quantity);
            return existing;
        }
        var line = new CartLine(product, quantity);
        _lines.Add(line);
        return line;
    }

    public void Remove(CartLine line) => _lines.Remove(line);

    public void Clear()
    {
        _lines.Clear();
        CartDiscount = null;
    }

    public void ApplyCartDiscount(Discount? discount) => CartDiscount = discount;
}
