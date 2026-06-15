using FluentAssertions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Domain.Pricing;
using Xunit;

namespace SupermarketPos.Tests;

public class PricingServiceTests
{
    private static Product P(decimal price, decimal vat, Unit unit = Unit.Piece, decimal stock = 100) =>
        new("BC", "Item", price, vat, unit, stock, 1);

    [Fact]
    public void VatExtraction_FromInclusiveGross_IsCorrect()
    {
        var cart = new Cart();
        cart.AddOrIncrement(P(120m, 0.20m), 1); // net 100, vat 20

        var groups = PricingService.VatBreakdown(cart.Lines);

        groups.Should().HaveCount(1);
        groups[0].Rate.Should().Be(0.20m);
        groups[0].Gross.Should().Be(120.00m);
        groups[0].Net.Should().Be(100.00m);
        groups[0].Vat.Should().Be(20.00m);
    }

    [Fact]
    public void VatBreakdown_GroupsMultipleRates()
    {
        var cart = new Cart();
        cart.AddOrIncrement(P(120m, 0.20m), 1); // net 100 / vat 20
        cart.AddOrIncrement(P(110m, 0.10m), 1); // net 100 / vat 10

        var groups = PricingService.VatBreakdown(cart.Lines);

        groups.Should().HaveCount(2);
        groups.Single(g => g.Rate == 0.10m).Vat.Should().Be(10.00m);
        groups.Single(g => g.Rate == 0.20m).Vat.Should().Be(20.00m);
    }

    [Fact]
    public void Rounding_IsAwayFromZero_ToTwoDecimals()
    {
        var cart = new Cart();
        cart.AddOrIncrement(P(1.00m, 0.20m), 1); // net = 1/1.2 = 0.8333.. -> 0.83, vat 0.17

        var g = PricingService.VatBreakdown(cart.Lines).Single();
        g.Net.Should().Be(0.83m);
        g.Vat.Should().Be(0.17m);
    }

    [Fact]
    public void WeighedItem_PricedByKilogram()
    {
        var cart = new Cart();
        cart.AddOrIncrement(P(20m, 0.01m, Unit.Kilogram), 0.350m); // 20 * 0.35 = 7.00

        var summary = PricingService.Summarize(cart);
        summary.GrossTotal.Should().Be(7.00m);
    }

    [Fact]
    public void Summarize_TotalsAreInternallyConsistent()
    {
        var cart = new Cart();
        cart.AddOrIncrement(P(120m, 0.20m), 2); // 240
        cart.AddOrIncrement(P(110m, 0.10m), 1); // 110

        var s = PricingService.Summarize(cart);

        s.GrossTotal.Should().Be(350.00m);
        (s.NetTotal + s.VatTotal).Should().Be(s.GrossTotal);
    }

    [Fact]
    public void CartDiscount_DistributedAcrossVatGroups_SumsToDiscountedGross()
    {
        var cart = new Cart();
        cart.AddOrIncrement(P(100m, 0.20m), 1); // gross 100
        cart.AddOrIncrement(P(100m, 0.10m), 1); // gross 100
        cart.ApplyCartDiscount(new PercentageDiscount(0.10m)); // 10% off -> 180

        var s = PricingService.Summarize(cart);

        s.GrossTotal.Should().Be(180.00m);
        s.VatBreakdown.Sum(g => g.Gross).Should().Be(180.00m);
        (s.NetTotal + s.VatTotal).Should().Be(180.00m);
        s.LineCharges.Sum(c => c.FinalGross).Should().Be(180.00m);
    }

    [Fact]
    public void Round2_Helper_UsesAwayFromZero()
    {
        Money.Round2(2.345m).Should().Be(2.35m);
        Money.Round2(2.344m).Should().Be(2.34m);
    }
}
