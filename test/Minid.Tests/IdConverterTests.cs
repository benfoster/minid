using Shouldly;

namespace Minid.Tests;

public class IdConverterTests
{
    private readonly IdConverter _converter;

    public IdConverterTests()
    {
        _converter = new IdConverter();
    }

    [Fact]
    public void Can_convert_to_string()
    {
        _converter.CanConvertTo(typeof(string)).ShouldBeTrue();

        var id = Id.NewId();
                
        _converter.ConvertTo(null, Thread.CurrentThread.CurrentCulture, id, typeof(string))
            .ShouldBe(id.ToString());
    }

    [Fact]
    public void Can_convert_from_string()
    {
        _converter.CanConvertFrom(typeof(string)).ShouldBeTrue();
                
        var id = Id.NewId();
        
        var decoded = _converter.ConvertFrom(null, Thread.CurrentThread.CurrentCulture, id.ToString());
        decoded.ShouldBe(id);
    }

    [Fact]
    public void Invalid_string_returns_null()
    {
        _converter.ConvertFrom(null, Thread.CurrentThread.CurrentCulture, "invalid").ShouldBeNull();
    }
}