namespace Minid.Tests;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;

public class NewtonsoftTests
{
    [Fact]
    public void Can_serialize_and_deserialize()
    {
        var poco = new Poco { Id = Id.NewId() };
       
        string json = JsonConvert.SerializeObject(poco);
        var doc = JObject.Parse(json);
        doc.GetValue("Id")!.ToString().ShouldBe(poco.Id.ToString());
    }

    class Poco
    {
        public Id Id { get; set; }
    }
}