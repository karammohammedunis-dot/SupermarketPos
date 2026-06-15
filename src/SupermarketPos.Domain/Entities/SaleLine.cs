using SupermarketPos.Domain.Common;

namespace SupermarketPos.Domain.Entities;

/// <summary>
/// An immutable snapshot of one sold line, frozen at checkout time. Prices/names are copied so
/// later product edits never rewrite history. <see cref="LineGross"/> is the final charged amount
/// (after line- and cart-level discounts), KDV-inclusive.
/// </summary>
public sealed class SaleLine
{
    public int Id { get; private set; }
    public int SaleId { get; private set; }
    public int ProductId { get; private set; }
    public string Barcode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }            // KDV-inclusive shelf price at sale time
    public decimal VatRate { get; private set; }
    public decimal Quantity { get; private set; }
    public string? DiscountDescription { get; private set; }
    public decimal LineGross { get; private set; }            // final charged gross (incl. VAT)

    private SaleLine() { } // EF

    public SaleLine(int productId, string barcode, string name, decimal unitPrice, decimal vatRate,
                    decimal quantity, string? discountDescription, decimal lineGross)
    {
        ProductId = productId;
        Barcode = barcode;
        Name = name;
        UnitPrice = Money.Round2(unitPrice);
        VatRate = vatRate;
        Quantity = quantity;
        DiscountDescription = discountDescription;
        LineGross = Money.Round2(lineGross);
    }

    public decimal NetAmount => Money.Round2(LineGross / (1 + VatRate));
    public decimal VatAmount => Money.Round2(LineGross - NetAmount);
}
