using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.Tests;

public class IMONumberTests
{
    [Theory]
    [InlineData("9074729")]
    [InlineData("1234567")]
    public void Valid_IMO_Passes_Checksum(string imo)
    {
        Assert.True(IMONumber.IsValid(imo));
    }

    [Theory]
    [InlineData("9074720")]
    [InlineData("1234568")]
    [InlineData("0000001")]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("abcdefg")]
    [InlineData("")]
    [InlineData(null)]
    public void Invalid_IMO_Fails_Checksum(string? imo)
    {
        Assert.False(IMONumber.IsValid(imo));
    }

    [Fact]
    public void Constructor_Throws_For_Invalid_IMO()
    {
        Assert.Throws<ArgumentException>(() => new IMONumber("0000001"));
    }

    [Fact]
    public void Constructor_Accepts_Valid_IMO()
    {
        var imo = new IMONumber("9074729");
        Assert.Equal("9074729", imo.ToString());
    }
}
