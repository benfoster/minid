using Shouldly;
namespace Minid.Tests;

public class IdTests
{
    [Fact]
    public void Can_encode_and_decode()
    {
        var id = Id.NewId();
        string encoded = id.ToString();
        
        Id.TryParse(encoded, out Id decoded).ShouldBeTrue();
        decoded.ShouldBe(id);
    }

    [Fact]
    public void Empty_guid_returns_padded_string()
    {
        var id = new Id(Guid.Empty);
        id.ToString().ShouldBe(new string('0', 26));
    }

    [Theory]
    [InlineData("473cr1y0ghbyc3m1yfbwvn3nxx")] // Original
    [InlineData("473crLy0ghbyc3mlyfbwvn3nxx")] // Test "l" and "L" instead of 1
    [InlineData("473criy0ghbyc3mIyfbwvn3nxx")] // Test "i" and "I" instead of 1
    [InlineData("473cr1yoghbyc3m1yfbwvn3nxx")] // Test "o" instead of 0
    [InlineData("473cr1yOghbyc3m1yfbwvn3nxx")] // Test "O" instead of 0
    public void Maps_ambiguous_characters_when_decoding(string encoded)
    {
        var guid = Guid.Parse("8108afcc-980f-438d-bdd7-51375fcf073a");
        var expected = new Id(guid); // 473cr1y0ghbyc3m1yfbwvn3nxx

        Id.TryParse(encoded, out Id decoded).ShouldBeTrue();
        decoded.ShouldBe(expected);
    }

    [Fact]
    public void Case_insensitive_when_decoding()
    {
        var id = Id.NewId();
        var encoded = id.ToString().ToUpperInvariant();

        Id.TryParse(encoded, out Id decoded).ShouldBeTrue();
        decoded.ShouldBe(id);
    }
}