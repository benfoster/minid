// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Minid;

BenchmarkRunner.Run(typeof(Program).Assembly);

[MemoryDiagnoser]
public class IdBenchmarks
{
    private const string Encoded = "473cr1y0ghbyc3m1yfbwvn3nxx";

    [Benchmark]
    public Id NewId() => Id.NewId();

    [Benchmark]
    public string IdToString() => Id.NewId().ToString();

    [Benchmark]
    public string GuidToString() => Guid.NewGuid().ToString();

    [Benchmark]
    public Id ParseId()
    {
        _ = Id.TryParse(Encoded, out Id decoded);
        return decoded;
    }
}