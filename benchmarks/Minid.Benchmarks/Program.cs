// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Minid;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[MemoryDiagnoser]
public class IdBenchmarks
{
    private const string Encoded = "473cr1y0ghbyc3m1yfbwvn3nxx";
    private const string EncodedWithPrefix = "cust_473cr1y0ghbyc3m1yfbwvn3nxx";

    [Benchmark]
    public Id NewId() => Id.NewId();

    [Benchmark]
    public string IdToString() => Id.NewId().ToString();

    // [Benchmark]
    // public string GuidToString() => Guid.NewGuid().ToString();

    [Benchmark]
    public Id ParseId()
    {
        _ = Id.TryParse(Encoded, out Id decoded);
        return decoded;
    }
}

[MemoryDiagnoser]
public class PrefixedIdBenchmarks
{
    private const string Prefix = "cust";
    private const string EncodedWithPrefix = Prefix + "_473cr1y0ghbyc3m1yfbwvn3nxx";

    [Benchmark]
    public Id NewPrefixedId() => Id.NewId(prefix: Prefix);

    [Benchmark]
    public string PrefixedIdToString() => Id.NewId(prefix: Prefix).ToString();

    // [Benchmark]
    // public Id ParsePrefixedId()
    // {
    //     _ = Id.TryParse(EncodedWithPrefix, out Id decoded);
    //     return decoded;
    // }
}