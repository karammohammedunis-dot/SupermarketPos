using FluentAssertions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;
using Xunit;

namespace SupermarketPos.Tests;

public class ProductStockTests
{
    private static Product P(decimal stock) => new("BC", "Item", 10m, 0.20m, Unit.Piece, stock, 1);

    [Fact]
    public void DecreaseStock_ReducesQuantity()
    {
        var p = P(10);
        p.DecreaseStock(3);
        p.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void DecreaseStock_ThrowsWhenExceedingStock()
    {
        var p = P(2);
        var act = () => p.DecreaseStock(3);
        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void DecreaseStock_ThrowsOnNonPositive()
    {
        var p = P(5);
        var act = () => p.DecreaseStock(0);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Restock_IncreasesQuantity()
    {
        var p = P(5);
        p.Restock(10);
        p.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void Construction_RejectsInvalidValues()
    {
        var badVat = () => new Product("BC", "X", 10m, 1.5m, Unit.Piece, 1, 1);
        badVat.Should().Throw<DomainException>();

        var emptyBarcode = () => new Product("", "X", 10m, 0.2m, Unit.Piece, 1, 1);
        emptyBarcode.Should().Throw<DomainException>();
    }
}
