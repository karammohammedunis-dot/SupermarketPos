using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SupermarketPos.Application.Dtos;
using SupermarketPos.Domain.Common;

namespace SupermarketPos.Infrastructure.Documents;

/// <summary>QuestPDF A4 layout for an X/Z or date-range sales report.</summary>
public sealed class ReportDocument : IDocument
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private readonly SalesReport _report;
    private readonly string _storeName;

    public ReportDocument(SalesReport report, string storeName)
    {
        _report = report;
        _storeName = storeName;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(t => t.FontSize(10));

            page.Header().Column(col =>
            {
                col.Item().Text(_storeName).Bold().FontSize(16);
                col.Item().Text(_report.Title).FontSize(13).Bold();
                col.Item().Text($"{_report.FromInclusive:dd.MM.yyyy} – {_report.ToExclusive.AddSeconds(-1):dd.MM.yyyy}").FontSize(9);
                col.Item().PaddingTop(6).LineHorizontal(1);
            });

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Spacing(4);
                col.Item().Text($"Satış Adedi: {_report.SaleCount}");
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Brüt Toplam").Bold();
                    r.ConstantItem(120).AlignRight().Text(Price(_report.GrossTotal)).Bold();
                });
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Matrah (Net)");
                    r.ConstantItem(120).AlignRight().Text(Price(_report.NetTotal));
                });
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Toplam KDV");
                    r.ConstantItem(120).AlignRight().Text(Price(_report.VatTotal));
                });

                col.Item().PaddingTop(10).Text("KDV Dağılımı").Bold().FontSize(12);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    table.Cell().Element(HeaderCell).Text("Oran");
                    table.Cell().Element(HeaderCell).AlignRight().Text("Matrah");
                    table.Cell().Element(HeaderCell).AlignRight().Text("KDV");
                    table.Cell().Element(HeaderCell).AlignRight().Text("Brüt");

                    foreach (var g in _report.VatBreakdown)
                    {
                        table.Cell().Element(BodyCell).Text($"%{g.Rate * 100:0.#}");
                        table.Cell().Element(BodyCell).AlignRight().Text(Price(g.Net));
                        table.Cell().Element(BodyCell).AlignRight().Text(Price(g.Vat));
                        table.Cell().Element(BodyCell).AlignRight().Text(Price(g.Gross));
                    }
                });

                col.Item().PaddingTop(10).Text("Ödeme Dağılımı").Bold().FontSize(12);
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text($"Nakit ({_report.PaymentSplit.CashCount} satış)");
                    r.ConstantItem(120).AlignRight().Text(Price(_report.PaymentSplit.CashTotal));
                });
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text($"Kredi Kartı ({_report.PaymentSplit.CardCount} satış)");
                    r.ConstantItem(120).AlignRight().Text(Price(_report.PaymentSplit.CardTotal));
                });
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Toplam").Bold();
                    r.ConstantItem(120).AlignRight().Text(Price(_report.PaymentSplit.TotalAmount)).Bold();
                });
            });

            page.Footer().AlignRight().Text(t =>
            {
                t.Span("Oluşturulma: ").FontSize(8);
                t.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm", Tr)).FontSize(8);
            });
        });
    }

    private static IContainer HeaderCell(IContainer c) =>
        c.BorderBottom(1).PaddingVertical(4).DefaultTextStyle(t => t.Bold());

    private static IContainer BodyCell(IContainer c) =>
        c.BorderBottom(0.5f).PaddingVertical(3);

    private static string Price(decimal value) => $"{value.ToString("N2", Tr)} {Money.CurrencySymbol}";
}
