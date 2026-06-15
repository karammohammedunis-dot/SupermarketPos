using FluentAssertions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Pricing;
using Xunit;

namespace SupermarketPos.Tests;

public class DiscountTests
{
    [Fact]
    public void Percentage_AppliesAndRounds()
    {
        new PercentageDiscount(0.10m).Apply(100m).Should().Be(90.00m);
        new PercentageDiscount(0.15m).Apply(33.33m).Should().Be(28.33m); // 28.3305 -> 28.33
    }

    [Fact]
    public void FixedAmount_NeverGoesNegative()
    {
        new FixedAmountDiscount(30m).Apply(20m).Should().Be(0m);
        new FixedAmountDiscount(5m).Apply(20m).Should().Be(15.00m);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.5)]
    public void Percentage_RejectsOutOfRange(double rate)
    {
        var act = () => new PercentageDiscount((decimal)rate);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FixedAmount_RejectsNonPositive()
    {
        var act = () => new FixedAmountDiscount(0m);
        act.Should().Throw<DomainException>();
    }
}

public class PaymentTests
{
    [Fact]
    public void Cash_ComputesChange()
    {
        var p = new CashPayment(73.50m, 100m);
        p.Change.Should().Be(26.50m);
        p.Amount.Should().Be(73.50m);
        p.Method.Should().Be("CASH");
    }

    [Fact]
    public void Cash_ThrowsWhenTenderedBelowTotal()
    {
        var act = () => new CashPayment(50m, 49.99m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Card_SettlesFullAmount()
    {
        var p = new CardPayment(120.00m);
        p.Amount.Should().Be(120.00m);
        p.Method.Should().Be("CARD");
    }
}
