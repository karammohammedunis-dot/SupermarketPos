using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Domain.Pricing;

namespace SupermarketPos.Application.Services;

/// <summary>
/// Checkout use-case. All money math is delegated to <see cref="PricingService"/>; stock decrement,
/// receipt numbering and persistence happen atomically inside one transaction so a stock shortfall
/// rolls the whole sale back.
/// </summary>
public sealed class SaleService : ISaleService
{
    private readonly IProductRepository _products;
    private readonly ISaleRepository _sales;
    private readonly IReceiptNumberGenerator _receiptNumbers;
    private readonly IUnitOfWork _uow;
    private readonly Func<DateTime> _clock;

    public SaleService(IProductRepository products, ISaleRepository sales,
                       IReceiptNumberGenerator receiptNumbers, IUnitOfWork uow,
                       Func<DateTime>? clock = null)
    {
        _products = products;
        _sales = sales;
        _receiptNumbers = receiptNumbers;
        _uow = uow;
        _clock = clock ?? (() => DateTime.Now);
    }

    public Task<Sale> CheckoutAsync(Cart cart, Payment payment, User cashier, CancellationToken ct = default)
    {
        if (cart is null || cart.IsEmpty)
            throw new DomainException("Cannot check out an empty cart.");
        if (payment is null)
            throw new DomainException("A payment is required.");
        if (cashier is null)
            throw new DomainException("A cashier is required.");

        var summary = PricingService.Summarize(cart);

        // The payment must cover the resolved gross total.
        if (payment.Amount != summary.GrossTotal)
            throw new DomainException("Payment amount does not match the sale total.");

        return _uow.ExecuteInTransactionAsync(async token =>
        {
            var now = _clock();
            var saleLines = new List<SaleLine>();

            foreach (var charge in summary.LineCharges)
            {
                var product = await _products.GetByIdAsync(charge.Line.Product.Id, token)
                              ?? throw new DomainException($"Product '{charge.Line.Product.Name}' no longer exists.");

                product.DecreaseStock(charge.Line.Quantity); // throws InsufficientStockException -> rollback

                saleLines.Add(new SaleLine(
                    productId: product.Id,
                    barcode: product.Barcode,
                    name: product.Name,
                    unitPrice: charge.Line.GrossUnitPrice,
                    vatRate: charge.Line.VatRate,
                    quantity: charge.Line.Quantity,
                    discountDescription: charge.Line.LineDiscount?.Describe(),
                    lineGross: charge.FinalGross));
            }

            var receiptNo = await _receiptNumbers.NextAsync(now, token);

            var sale = new Sale(
                receiptNo: receiptNo,
                cashierId: cashier.Id,
                cashierName: cashier.DisplayName,
                lines: saleLines,
                grossTotal: summary.GrossTotal,
                netTotal: summary.NetTotal,
                vatTotal: summary.VatTotal,
                payment: payment,
                createdAt: now);

            await _sales.AddAsync(sale, token);
            await _uow.SaveChangesAsync(token);
            return sale;
        }, ct);
    }
}
