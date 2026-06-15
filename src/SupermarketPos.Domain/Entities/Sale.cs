using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Pricing;

namespace SupermarketPos.Domain.Entities;

/// <summary>
/// A completed, persisted sale. Built from a resolved <see cref="SaleSummary"/> plus a
/// <see cref="Payment"/>, it stores immutable line snapshots, the headline totals, and the
/// payment outcome. The VAT breakdown is recomputed from the line snapshots on demand.
/// </summary>
public sealed class Sale
{
    private readonly List<SaleLine> _lines = new();

    public int Id { get; private set; }
    public string ReceiptNo { get; private set; } = string.Empty;  // e.g. 2026-000123
    public DateTime CreatedAt { get; private set; }
    public int CashierId { get; private set; }
    public string CashierName { get; private set; } = string.Empty;

    public IReadOnlyList<SaleLine> Lines => _lines;

    public decimal GrossTotal { get; private set; }
    public decimal NetTotal { get; private set; }
    public decimal VatTotal { get; private set; }

    public string PaymentMethod { get; private set; } = string.Empty; // CASH | CARD
    public decimal AmountPaid { get; private set; }
    public decimal? CashTendered { get; private set; }
    public decimal? ChangeGiven { get; private set; }

    private Sale() { } // EF

    public Sale(string receiptNo, int cashierId, string cashierName,
                IEnumerable<SaleLine> lines, decimal grossTotal, decimal netTotal, decimal vatTotal,
                Payment payment, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(receiptNo))
            throw new DomainException("Receipt number is required.");
        ReceiptNo = receiptNo;
        CashierId = cashierId;
        CashierName = cashierName;
        _lines.AddRange(lines);
        if (_lines.Count == 0) throw new DomainException("A sale must have at least one line.");
        GrossTotal = Money.Round2(grossTotal);
        NetTotal = Money.Round2(netTotal);
        VatTotal = Money.Round2(vatTotal);
        CreatedAt = createdAt;

        PaymentMethod = payment.Method;
        AmountPaid = payment.Amount;
        if (payment is CashPayment cash)
        {
            CashTendered = cash.Tendered;
            ChangeGiven = cash.Change;
        }
    }

    /// <summary>Per-rate VAT breakdown, recomputed from the persisted line snapshots.</summary>
    public IReadOnlyList<VatGroup> VatBreakdown =>
        PricingService.ExtractGroups(_lines.Select(l => (l.VatRate, l.LineGross)));
}
