using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
namespace Minid.Tests;

public class SerializationTests
{
    [Fact]
    public void Serializes_with_newtonsoft()
    {
        var poco = new Poco { Id = Id.NewId() };

        string json = JsonConvert.SerializeObject(poco);
        var jObject = JObject.Parse(json);
        jObject["Id"].ShouldBe(poco.Id.ToString());
    }

    class Poco
    {
        public Id Id { get; set; }
    }
}