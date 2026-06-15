using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Infrastructure.Documents;

/// <summary>QuestPDF thermal-style (80mm) receipt for a single sale, TR fiş layout.</summary>
public sealed class ReceiptDocument : IDocument
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private readonly Sale _sale;
    private readonly AppSettings _settings;

    public ReceiptDocument(Sale sale, AppSettings settings)
    {
        _sale = sale;
        _settings = settings;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.ContinuousSize(80, QuestPDF.Infrastructure.Unit.Millimetre);
            page.Margin(6);
            page.DefaultTextStyle(t => t.FontSize(8).FontFamily(Fonts.Consolas));

            page.Content().Column(col =>
            {
                col.Item().AlignCenter().Text(_settings.StoreName).Bold().FontSize(11);
                if (!string.IsNullOrWhiteSpace(_settings.StoreAddress))
                    col.Item().AlignCenter().Text(_settings.StoreAddress).FontSize(7);
                col.Item().PaddingVertical(3).LineHorizontal(0.5f);

                col.Item().Text($"Fiş No : {_sale.ReceiptNo}");
                col.Item().Text($"Tarih  : {_sale.CreatedAt.ToString("dd.MM.yyyy HH:mm", Tr)}");
                col.Item().Text($"Kasiyer: {_sale.CashierName}");
                col.Item().PaddingVertical(3).LineHorizontal(0.5f);

                foreach (var line in _sale.Lines)
                {
                    col.Item().Text(line.Name).Bold();
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"  {Qty(line)} x {Price(line.UnitPrice)}").FontSize(7);
                        r.ConstantItem(60).AlignRight().Text(Price(line.LineGross));
                    });
                    if (!string.IsNullOrWhiteSpace(line.DiscountDescription))
                        col.Item().Text($"  İndirim {line.DiscountDescription}").FontSize(7).Italic();
                }

                col.Item().PaddingVertical(3).LineHorizontal(0.5f);

                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("TOPLAM").Bold().FontSize(11);
                    r.ConstantItem(80).AlignRight().Text(Price(_sale.GrossTotal)).Bold().FontSize(11);
                });

                col.Item().PaddingTop(3).Text("KDV Dağılımı").Bold().FontSize(8);
                foreach (var g in _sale.VatBreakdown)
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"KDV %{g.Rate * 100:0.#}  (Matrah {Price(g.Net)})").FontSize(7);
                        r.ConstantItem(60).AlignRight().Text(Price(g.Vat)).FontSize(7);
                    });
                }
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Toplam KDV").FontSize(7);
                    r.ConstantItem(60).AlignRight().Text(Price(_sale.VatTotal)).FontSize(7);
                });

                col.Item().PaddingVertical(3).LineHorizontal(0.5f);

                col.Item().Row(r =>
                {
                    r.RelativeItem().Text(_sale.PaymentMethod == "CASH" ? "Nakit" : "Kredi Kartı");
                    r.ConstantItem(60).AlignRight().Text(Price(_sale.AmountPaid));
                });
                if (_sale.CashTendered is { } tendered)
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Alınan");
                        r.ConstantItem(60).AlignRight().Text(Price(tendered));
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Para Üstü").Bold();
                        r.ConstantItem(60).AlignRight().Text(Price(_sale.ChangeGiven ?? 0m)).Bold();
                    });
                }

                col.Item().PaddingTop(6).AlignCenter().Text("Bizi tercih ettiğiniz için teşekkürler!").FontSize(7).Italic();
            });
        });
    }

    private static string Qty(SaleLine l) =>
        l.Quantity == Math.Truncate(l.Quantity)
            ? l.Quantity.ToString("0", Tr)
            : l.Quantity.ToString("0.000", Tr);

    private static string Price(decimal value) => $"{value.ToString("N2", Tr)} {Money.CurrencySymbol}";
}
