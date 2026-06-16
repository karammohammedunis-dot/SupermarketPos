using System.Text;
using SupermarketPos.Application.Abstractions;

namespace SupermarketPos.Application.Services;

/// <summary>
/// Normalizes scanner / keyboard input and decodes in-store variable-measure barcodes.
///
/// Weight-barcode scheme (EAN-13, configurable convention used here):
///   digit 1-2  : prefix (starts with '2' = in-store variable measure)
///   digit 3-7  : PLU / item code (5 digits) — identifies the product
///   digit 8-12 : weight in grams (5 digits, e.g. 01250 = 1.250 kg)
///   digit 13   : EAN-13 check digit
/// </summary>
public sealed class BarcodeService : IBarcodeService
{
    public string Normalize(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        var sb = new StringBuilder(raw.Length);
        foreach (var ch in raw)
        {
            if (char.IsControl(ch)) continue; // CR, LF, tab from scanner
            sb.Append(ch);
        }
        return sb.ToString().Trim();
    }

    public bool IsWeightBarcode(string code) =>
        !string.IsNullOrEmpty(code) && code.Length == 13 && code[0] == '2' && code.All(char.IsDigit);

    public WeightBarcode? ParseWeight(string code)
    {
        if (!IsWeightBarcode(code)) return null;

        var expectedCheck = Ean13CheckDigit(code.Substring(0, 12));
        if (expectedCheck != code[12] - '0') return null;

        var plu = code.Substring(2, 5);
        var grams = int.Parse(code.Substring(7, 5));
        var weightKg = grams / 1000m;
        if (weightKg <= 0) return null;

        return new WeightBarcode(plu, weightKg);
    }

    public int Ean13CheckDigit(string twelveDigits)
    {
        if (twelveDigits is null || twelveDigits.Length != 12 || !twelveDigits.All(char.IsDigit))
            throw new ArgumentException("Exactly 12 digits are required.", nameof(twelveDigits));

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var d = twelveDigits[i] - '0';
            sum += (i % 2 == 0) ? d : d * 3; // positions 1,3,.. weight 1; 2,4,.. weight 3
        }
        return (10 - (sum % 10)) % 10;
    }
}
