using QuestPDF.Fluent;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Application.Dtos;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Infrastructure.Documents;

public sealed class ReceiptService : IReceiptService
{
    private readonly ISettingsRepository _settings;
    public ReceiptService(ISettingsRepository settings) => _settings = settings;

    public async Task<byte[]> RenderPdfAsync(Sale sale, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        return new ReceiptDocument(sale, settings).GeneratePdf();
    }

    public async Task SavePdfAsync(Sale sale, string path, CancellationToken ct = default)
    {
        var bytes = await RenderPdfAsync(sale, ct);
        await File.WriteAllBytesAsync(path, bytes, ct);
    }
}

public sealed class ShelfLabelService : IShelfLabelService
{
    private readonly ISettingsRepository _settings;
    public ShelfLabelService(ISettingsRepository settings) => _settings = settings;

    public async Task<byte[]> RenderPdfAsync(IReadOnlyList<Product> products, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        return new ShelfLabelDocument(products, settings.StoreName).GeneratePdf();
    }

    public async Task SavePdfAsync(IReadOnlyList<Product> products, string path, CancellationToken ct = default)
    {
        var bytes = await RenderPdfAsync(products, ct);
        await File.WriteAllBytesAsync(path, bytes, ct);
    }
}

public sealed class ReportPdfService : IReportPdfService
{
    private readonly ISettingsRepository _settings;
    public ReportPdfService(ISettingsRepository settings) => _settings = settings;

    public async Task<byte[]> RenderPdfAsync(SalesReport report, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        return new ReportDocument(report, settings.StoreName).GeneratePdf();
    }

    public async Task SavePdfAsync(SalesReport report, string path, CancellationToken ct = default)
    {
        var bytes = await RenderPdfAsync(report, ct);
        await File.WriteAllBytesAsync(path, bytes, ct);
    }
}
