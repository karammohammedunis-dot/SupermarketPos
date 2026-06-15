using System.Globalization;

namespace SupermarketPos.Wpf.Common;

/// <summary>Formats decimals as TRY money strings for the UI (mono digits, ₺ suffix).</summary>
public static class MoneyText
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public static string Format(decimal value) =>
        $"{value.ToString("N2", Tr)} {SupermarketPos.Domain.Common.Money.CurrencySymbol}";

    public static string Qty(decimal qty, bool weighed) =>
        weighed ? qty.ToString("0.000", Tr) : qty.ToString("0", Tr);
}
