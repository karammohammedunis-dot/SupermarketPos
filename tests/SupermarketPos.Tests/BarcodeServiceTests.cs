using FluentAssertions;
using SupermarketPos.Application.Services;
using Xunit;

namespace SupermarketPos.Tests;

public class BarcodeServiceTests
{
    private readonly BarcodeService _svc = new();

    [Fact]
    public void Normalize_StripsControlCharsAndWhitespace()
    {
        _svc.Normalize("  869000\r\n0000017 \t").Should().Be("8690000000017");
    }

    [Fact]
    public void Ean13CheckDigit_IsCorrect()
    {
        // 200000100000 -> check 7  (PLU 00001 produce label)
        _svc.Ean13CheckDigit("200000100000").Should().Be(7);
    }

    [Fact]
    public void ParseWeight_DecodesPluAndWeight()
    {
        // 2 0 00001 01250 5  -> PLU 00001, 1250 g = 1.250 kg
        var result = _svc.ParseWeight("2000001012505");

        result.Should().NotBeNull();
        result!.Plu.Should().Be("00001");
        result.WeightKg.Should().Be(1.250m);
    }

    [Fact]
    public void ParseWeight_RejectsBadCheckDigit()
    {
        _svc.ParseWeight("2000001012500").Should().BeNull(); // check should be 5, not 0
    }

    [Theory]
    [InlineData("8690000000017")] // normal product (prefix 8)
    [InlineData("12345")]          // too short
    [InlineData("2abc001012505")]  // non-digits
    public void IsWeightBarcode_OnlyTrueForVariableMeasure(string code)
    {
        _svc.IsWeightBarcode(code).Should().BeFalse();
    }

    [Fact]
    public void ParseWeight_RejectsZeroWeight()
    {
        // PLU 00001, weight 00000 -> the shelf label code, not a real weighed sale
        _svc.ParseWeight("2000001000007").Should().BeNull();
    }
}
