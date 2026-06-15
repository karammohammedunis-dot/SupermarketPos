using SupermarketPos.Domain.Common;

namespace SupermarketPos.Domain.Entities;

/// <summary>
/// Single-row store configuration: receipt header info, UI language, low-stock threshold and the
/// configurable list of KDV rates. VAT rates are NEVER hardcoded in logic — they live here and on
/// each product.
/// </summary>
public sealed class AppSettings
{
    public int Id { get; private set; } = 1;
    public string StoreName { get; private set; } = "Süpermarket";
    public string StoreAddress { get; private set; } = string.Empty;
    public string Language { get; private set; } = "tr";          // tr | en
    public decimal LowStockThreshold { get; private set; } = 5m;
    public string VatRatesCsv { get; private set; } = "0.01,0.10,0.20";

    private AppSettings() { } // EF

    public AppSettings(string storeName, string storeAddress, string language,
                       decimal lowStockThreshold, IEnumerable<decimal> vatRates)
    {
        Update(storeName, storeAddress, language, lowStockThreshold, vatRates);
    }

    public IReadOnlyList<decimal> VatRates =>
        VatRatesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Select(decimal.Parse)
                   .ToList();

    public void Update(string storeName, string storeAddress, string language,
                       decimal lowStockThreshold, IEnumerable<decimal> vatRates)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new DomainException("Store name cannot be empty.");
        if (lowStockThreshold < 0)
            throw new DomainException("Low-stock threshold cannot be negative.");
        var rates = vatRates?.ToList() ?? new List<decimal>();
        if (rates.Count == 0)
            throw new DomainException("At least one VAT rate is required.");
        if (rates.Any(r => r < 0 || r > 1))
            throw new DomainException("VAT rates must be between 0 and 1.");

        StoreName = storeName.Trim();
        StoreAddress = storeAddress?.Trim() ?? string.Empty;
        Language = string.IsNullOrWhiteSpace(language) ? "tr" : language.Trim().ToLowerInvariant();
        LowStockThreshold = lowStockThreshold;
        VatRatesCsv = string.Join(',', rates.Select(r => r.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }
}
