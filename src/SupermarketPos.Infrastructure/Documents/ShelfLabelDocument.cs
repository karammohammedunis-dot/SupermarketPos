using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Infrastructure.Documents;

/// <summary>QuestPDF A4 sheet of shelf labels (3 per row): name, price and an EAN-13 barcode.</summary>
public sealed class ShelfLabelDocument : IDocument
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private readonly IReadOnlyList<Product> _products;
    private readonly string _storeName;

    public ShelfLabelDocument(IReadOnlyList<Product> products, string storeName)
    {
        _products = products;
        _storeName = storeName;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(t => t.FontSize(9));

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                foreach (var p in _products)
                    table.Cell().Padding(5).Element(c => Label(c, p));
            });
        });
    }

    private void Label(IContainer container, Product product)
    {
        container.Border(1).BorderColor("#CBD5E1").Padding(8).Column(col =>
        {
            col.Item().Text(_storeName).FontSize(7).FontColor("#64748B");
            col.Item().PaddingTop(2).Text(product.Name).Bold().FontSize(12);
            col.Item().Text($"{product.UnitPrice.ToString("N2", Tr)} {Money.CurrencySymbol}" +
                            (product.Unit == SupermarketPos.Domain.Common.Unit.Kilogram ? " / kg" : ""))
                      .FontSize(15).Bold().FontColor("#2563EB");

            var png = BarcodeImage.TryRenderEan13(product.Barcode);
            if (png is not null)
                col.Item().PaddingTop(4).AlignCenter().Height(48).Image(png).FitHeight();
            else
                col.Item().PaddingTop(4).AlignCenter().Text(product.Barcode).FontSize(8);
        });
    }
}
