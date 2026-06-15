using System.Text;
using SupermarketPos.Application.Abstractions;

namespace SupermarketPos.Application.Services;

/// <summary>
/// Normalizes scanner / keyboard input: strips the Enter terminator and whitespace, and drops
/// non-printable control characters that some scanners emit.
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
}
