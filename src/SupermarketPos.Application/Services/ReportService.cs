using SupermarketPos.Application.Abstractions;
using SupermarketPos.Application.Dtos;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Domain.Pricing;

namespace SupermarketPos.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly ISaleRepository _sales;

    public ReportService(ISaleRepository sales) => _sales = sales;

    public Task<SalesReport> GetXReportAsync(DateTime day, CancellationToken ct = default) =>
        BuildAsync("X-Report", day.Date, day.Date.AddDays(1), ct);

    public Task<SalesReport> GetZReportAsync(DateTime day, CancellationToken ct = default) =>
        BuildAsync("Z-Report", day.Date, day.Date.AddDays(1), ct);

    public Task<SalesReport> GetRangeReportAsync(DateTime fromInclusive, DateTime toExclusive, string title, CancellationToken ct = default) =>
        BuildAsync(title, fromInclusive, toExclusive, ct);

    public Task<IReadOnlyList<Sale>> GetSalesAsync(DateTime fromInclusive, DateTime toExclusive, CancellationToken ct = default) =>
        _sales.GetByDateRangeAsync(fromInclusive, toExclusive, ct);

    private async Task<SalesReport> BuildAsync(string title, DateTime from, DateTime to, CancellationToken ct)
    {
        var sales = await _sales.GetByDateRangeAsync(from, to, ct);

        var gross = Money.Round2(sales.Sum(s => s.GrossTotal));
        var net = Money.Round2(sales.Sum(s => s.NetTotal));
        var vat = Money.Round2(sales.Sum(s => s.VatTotal));

        var breakdown = PricingService.ExtractGroups(
            sales.SelectMany(s => s.Lines).Select(l => (l.VatRate, l.LineGross)));

        var cash = sales.Where(s => s.PaymentMethod == "CASH").ToList();
        var card = sales.Where(s => s.PaymentMethod == "CARD").ToList();
        var split = new PaymentSplit(
            CashCount: cash.Count,
            CashTotal: Money.Round2(cash.Sum(s => s.AmountPaid)),
            CardCount: card.Count,
            CardTotal: Money.Round2(card.Sum(s => s.AmountPaid)));

        return new SalesReport(title, from, to, sales.Count, gross, net, vat, breakdown, split);
    }
}
