namespace Minid.Benchmarks;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class IdBenchmarks
{
    private const string Encoded = "473cr1y0ghbyc3m1yfbwvn3nxx";

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
    private const string EncodedWithPrefix = "cust_473cr1y0ghbyc3m1yfbwvn3nxx";

    [Benchmark]
    public Id NewPrefixedId() => Id.NewId(prefix: Prefix);

    [Benchmark]
    public string PrefixedIdToString() => Id.NewId(prefix: Prefix).ToString();

    [Benchmark]
    public Id ParsePrefixedId()
    {
        _ = Id.TryParse(EncodedWithPrefix, out Id decoded);
        return decoded;
    }

    [Benchmark]
    public Id ParseIdKnownPrefix()
    {
        _ = Id.TryParse(EncodedWithPrefix, Prefix, out Id decoded);
        return decoded;
    }
}

[MemoryDiagnoser]
public class IndexOfBenchmarks
{
    [Benchmark]
    public int IndexOf()
    {
        ReadOnlySpan<char> id = "cust_473cr1y0ghbyc3m1yfbwvn3nxx";
        return id.IndexOf("_");
    }

    [Benchmark]
    public int LastIndexOf()
    {
        ReadOnlySpan<char> id = "cust_473cr1y0ghbyc3m1yfbwvn3nxx";
        return id[..26].LastIndexOf('_');
    }
}