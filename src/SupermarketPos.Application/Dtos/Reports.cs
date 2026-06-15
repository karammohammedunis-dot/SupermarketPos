using SupermarketPos.Domain.Pricing;

namespace SupermarketPos.Application.Dtos;

/// <summary>Sales split by payment method.</summary>
public sealed record PaymentSplit(int CashCount, decimal CashTotal, int CardCount, decimal CardTotal)
{
    public int TotalCount => CashCount + CardCount;
    public decimal TotalAmount => CashTotal + CardTotal;
}

/// <summary>
/// Running totals for a period. The X-report covers the current open day (not closed);
/// the Z-report covers a closed day. Both share this shape.
/// </summary>
public sealed record SalesReport(
    string Title,
    DateTime FromInclusive,
    DateTime ToExclusive,
    int SaleCount,
    decimal GrossTotal,
    decimal NetTotal,
    decimal VatTotal,
    IReadOnlyList<VatGroup> VatBreakdown,
    PaymentSplit PaymentSplit);
