namespace SupermarketPos.Domain.Pricing;

/// <summary>
/// VAT (KDV) totals for a single rate. Gross is KDV-inclusive; Net is the pre-tax base;
/// Vat is the tax portion (Gross - Net). All values are already rounded to 2 decimals.
/// </summary>
public sealed record VatGroup(decimal Rate, decimal Gross, decimal Net, decimal Vat);
