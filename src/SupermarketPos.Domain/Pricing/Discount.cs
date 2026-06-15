using SupermarketPos.Domain.Common;

namespace SupermarketPos.Domain.Pricing;

/// <summary>
/// Discount strategy (polymorphism #1). A discount transforms a gross amount into a smaller,
/// never-negative gross amount. Concrete strategies decide how.
/// </summary>
public abstract class Discount
{
    public abstract decimal Apply(decimal gross);
    public abstract string Describe();
}

/// <summary>Percentage off, rate in the range 0..1 (e.g. 0.10 = 10% off).</summary>
public sealed class PercentageDiscount : Discount
{
    public decimal Rate { get; }

    public PercentageDiscount(decimal rate)
    {
        if (rate < 0 || rate > 1)
            throw new DomainException("Percentage discount rate must be between 0 and 1.");
        Rate = rate;
    }

    public override decimal Apply(decimal gross) => Money.Round2(gross * (1 - Rate));

    public override string Describe() => $"-%{Rate * 100:0.#}";
}

/// <summary>A fixed amount off the gross, clamped so the result never drops below zero.</summary>
public sealed class FixedAmountDiscount : Discount
{
    public decimal Amount { get; }

    public FixedAmountDiscount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Fixed discount amount must be positive.");
        Amount = Money.Round2(amount);
    }

    public override decimal Apply(decimal gross) => Math.Max(0m, Money.Round2(gross - Amount));

    public override string Describe() => $"-{Amount:0.00}{Money.CurrencySymbol}";
}
