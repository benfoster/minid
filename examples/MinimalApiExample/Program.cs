using MinimalApiExampleDemo;
using System.Globalization;
using Minid;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/customers", () =>
{
    return new
    {
        id = Id.NewId("cus")
    };
});

app.MapGet("/customers/{id}", (Id id) =>
{
    return new
    {
        id
    };
});

app.MapPost("/customers/{id}/disable", (DisableRequest disableRequest) =>
{
    return disableRequest;
});

app.Run();


namespace MinimalApiExampleDemo
{
    public class DisableRequest
    {
        public Id OperatorId { get; set; }
    }
}