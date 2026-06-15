namespace SupermarketPos.Domain.Common;

/// <summary>
/// Money helpers. ALL monetary values in the system are <see cref="decimal"/> (never float/double),
/// currency is TRY app-wide, and rounding is to 2 decimals using AwayFromZero (Turkish retail convention).
/// This is the single rounding primitive the whole domain depends on.
/// </summary>
public static class Money
{
    public const string CurrencyCode = "TRY";
    public const string CurrencySymbol = "₺";

    public static decimal Round2(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
