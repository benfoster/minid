using System.Text.Json;
using Shouldly;
namespace Minid.Tests;

public class IdJsonConverterTests
{
    [Fact]
    public void Can_serialize_and_deserialize()
    {
        var poco = new Poco { Id = Id.NewId() };
       
        string json = JsonSerializer.Serialize(poco);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Id").GetString().ShouldBe(poco.Id.ToString());
    }

    class Poco
    {
        public Id Id { get; set; }
    }
}