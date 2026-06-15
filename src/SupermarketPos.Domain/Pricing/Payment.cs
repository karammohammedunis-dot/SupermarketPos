using SupermarketPos.Domain.Common;

namespace SupermarketPos.Domain.Pricing;

/// <summary>
/// Payment strategy (polymorphism #2). Each concrete payment knows how to settle a sale total
/// and reports its method. Cash additionally computes change from the tendered amount.
/// </summary>
public abstract class Payment
{
    public decimal Amount { get; protected set; } // amount applied to the sale
    public abstract string Method { get; }
}

public sealed class CashPayment : Payment
{
    public decimal Tendered { get; }
    public decimal Change { get; }

    public CashPayment(decimal total, decimal tendered)
    {
        if (total < 0) throw new DomainException("Total cannot be negative.");
        if (tendered < total) throw new DomainException("Insufficient cash tendered.");
        Amount = Money.Round2(total);
        Tendered = Money.Round2(tendered);
        Change = Money.Round2(tendered - total);
    }

    public override string Method => "CASH";
}

public sealed class CardPayment : Payment
{
    public CardPayment(decimal total)
    {
        if (total < 0) throw new DomainException("Total cannot be negative.");
        Amount = Money.Round2(total);
        // Simulated approval — no real terminal integration.
    }

    public override string Method => "CARD";
}
