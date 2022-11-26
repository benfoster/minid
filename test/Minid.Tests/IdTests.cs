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

    [Fact]
    public void Can_prefix_id()
    {
        var id = Id.NewId(prefix: "cust");
        var encoded = id.ToString();
        encoded.ShouldStartWith("cust_");
        encoded.Length.ShouldBe(31); // prefix + separator + 26 encoded guid
    }

    [Fact]
    public void Can_decode_prefixed_id()
    {
        var id = Id.NewId(prefix: "cust");
        var encoded = id.ToString();

        Id.TryParse(encoded, out Id decoded).ShouldBeTrue();
        decoded.ShouldBe(id);
        decoded.ToString().ShouldStartWith("cust_");
    }

    [Fact]
    public void Can_decode_known_prefixed_id()
    {
        var id = Id.NewId(prefix: "cust");
        var encoded = id.ToString();

        Id.TryParse(encoded, "cust", out Id decoded).ShouldBeTrue();
        decoded.ShouldBe(id);
        decoded.ToString().ShouldStartWith("cust_");
    }

    [Theory]
    [InlineData("473cr1y0ghbyc3m1yfbwvn3nxx", true)]
    [InlineData("473cr1y", false)]
    [InlineData("ord_473cr1y0ghbyc3m1yfbwvn3nxx", true)]
    [InlineData("ord_cr1y0ghbyc3m1yfbwvn3nxx", false)]
    [InlineData("473cr1y0ghbyc3m1yfbwvn3nxx0ghbyc3m1yfbwvn3nxx", false)]
    public void Validates_length_when_decoding(string encoded, bool isValid)
    {
        Id.TryParse(encoded, out Id _).ShouldBe(isValid);
    }

    [Theory]
    [InlineData("cust_473cr1y0ghbyc3m1yfbwvn3nxx", "cust", true)]
    [InlineData("foo_473cr1y0ghbyc3m1yfbwvn3nxx", "cust", false)]
    public void Validates_known_prefix(string encoded, string knownPrefix, bool isValid)
    {
        Id.TryParse(encoded, knownPrefix, out Id _).ShouldBe(isValid);
    }

    [Theory]
    [InlineData("473cr1y0ghbyc3m1yfbwvn3nxx")]
    [InlineData("acc_473cr1y0ghbyc3m1yfbwvn3nxx")]
    public void Can_parse(string encoded)
    {
        Id.Parse(encoded);
    }

    [Fact]
    public void Can_parse_throws_if_invalid()
    {
        Should.Throw<ArgumentException>(() => Id.Parse("invalid"));
    }

    [Fact]
    public void Can_parse_with_prefix()
    {
        Id.Parse("acc_473cr1y0ghbyc3m1yfbwvn3nxx", "acc");
    }

    [Fact]
    public void Can_parse_with_prefix_throws_if_invalid()
    {
        Should.Throw<ArgumentException>(() => Id.Parse("invalid", "acc"));
    }
}