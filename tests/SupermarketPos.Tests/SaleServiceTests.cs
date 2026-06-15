using FluentAssertions;
using SupermarketPos.Application.Services;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Domain.Pricing;
using SupermarketPos.Infrastructure.Persistence;
using Xunit;

namespace SupermarketPos.Tests;

public class SaleServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 15, 10, 0, 0);

    private static (SaleService svc, TestDb db, User cashier) Build(TestDb db)
    {
        db.Context.Categories.Add(new Category("Test"));
        db.Context.SaveChanges();

        var svc = new SaleService(
            new ProductRepository(db.Context),
            new SaleRepository(db.Context),
            new ReceiptNumberGenerator(db.Context),
            new UnitOfWork(db.Context),
            () => FixedNow);

        var cashier = new User("kasa", "hash", "Kasiyer", UserRole.Cashier);
        return (svc, db, cashier);
    }

    private static Product SeedProduct(TestDb db, decimal price, decimal stock)
    {
        var p = new Product($"BC{Guid.NewGuid():N}", "Item", price, 0.20m, Unit.Piece, stock, 1);
        db.Context.Products.Add(p);
        db.Context.SaveChanges();
        return p;
    }

    [Fact]
    public async Task Checkout_DecrementsStock_AndPersistsSale()
    {
        using var db = new TestDb();
        var (svc, _, cashier) = Build(db);
        var product = SeedProduct(db, 10m, 5m);

        var cart = new Cart();
        cart.AddOrIncrement(product, 2);
        var total = PricingService.GrossTotal(cart);

        var sale = await svc.CheckoutAsync(cart, new CashPayment(total, total), cashier);

        sale.ReceiptNo.Should().Be("2026-000001");
        sale.GrossTotal.Should().Be(20.00m);

        using var verify = db.NewContext();
        verify.Products.Single().StockQuantity.Should().Be(3m);
        verify.Sales.Should().HaveCount(1);
    }

    [Fact]
    public async Task Checkout_InsufficientStock_RollsBack()
    {
        using var db = new TestDb();
        var (svc, _, cashier) = Build(db);
        var product = SeedProduct(db, 10m, 1m);

        var cart = new Cart();
        cart.AddOrIncrement(product, 2);
        var total = PricingService.GrossTotal(cart);

        var act = async () => await svc.CheckoutAsync(cart, new CashPayment(total, total), cashier);

        await act.Should().ThrowAsync<InsufficientStockException>();

        using var verify = db.NewContext();
        verify.Products.Single().StockQuantity.Should().Be(1m); // unchanged
        verify.Sales.Should().BeEmpty();                         // nothing persisted
    }

    [Fact]
    public async Task ReceiptNumbers_AreSequential()
    {
        using var db = new TestDb();
        var (svc, _, cashier) = Build(db);
        var product = SeedProduct(db, 10m, 100m);

        async Task<string> Sell()
        {
            var cart = new Cart();
            cart.AddOrIncrement(product, 1);
            var total = PricingService.GrossTotal(cart);
            var sale = await svc.CheckoutAsync(cart, new CardPayment(total), cashier);
            return sale.ReceiptNo;
        }

        (await Sell()).Should().Be("2026-000001");
        (await Sell()).Should().Be("2026-000002");
        (await Sell()).Should().Be("2026-000003");
    }
}
