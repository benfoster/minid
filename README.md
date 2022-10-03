# Minid

[![NuGet](https://img.shields.io/nuget/v/minid.svg)](https://www.nuget.org/packages/minid) 
[![NuGet](https://img.shields.io/nuget/dt/minid.svg)](https://www.nuget.org/packages/minid)
[![License](https://img.shields.io/:license-mit-blue.svg)](https://benfoster.mit-license.org/)

![Build](https://github.com/benfoster/minid/workflows/Build/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/benfoster/minid/badge.svg?branch=main)](https://coveralls.io/github/benfoster/minid?branch=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=benfoster_minid&metric=alert_status)](https://sonarcloud.io/dashboard?id=benfoster_minid)
[![GuardRails badge](https://api.guardrails.io/v2/badges/benfoster/minid.svg?token=59af717aeba71bc862995cd302659f0e511ebe43ff28ee6df11fe8669b15dc1d&provider=github)](https://dashboard.guardrails.io/gh/benfoster/repos/146839)

Minid generates human-readable, URL-friendly, unique identifiers that are computed in-memory.

- Safe without encoding (uses only characters from ASCII)
- Avoids ambiguous characters (i/I/l/L/o/O/0)
- Easy for humans to read and pronounce
- Supports full UUID range (128 bits)
- Safe for URLs and file names
- Case-insensitive
- 30% smaller than Guid's default string format
- Supports formatting with prefixes

### Example

The `Guid` `8108afcc-980f-438d-bdd7-51375fcf073a` converted to a Minid `Id` is encoded as `473cr1y0ghbyc3m1yfbwvn3nxx`.

## Motivation

The motivation for this library came from a need to return readable "resource" identifiers in our APIs that could be computed quickly (no database lookups). In .NET the [Guid Struct](https://learn.microsoft.com/en-us/dotnet/api/system.guid?view=net-7.0) meets the uniqueness and computational requirements but its string representation is not optimal in terms of size and format.

With [Base32 encoding](https://en.wikipedia.org/wiki/Base32) we can encode a 128-bit `Guid` into a 26-character string. My original implementation was based on a Base32 encoder ported from [this Java implementation](https://github.com/google/google-authenticator-android/blob/master/java/com/google/android/apps/authenticator/util/Base32String.java).

I later updated this to use [Kristian Hellang](https://github.com/khellang)'s "CompactGuid" implementation which was far more performant and had better support for ambiguous characters 🙇‍♂️

## Usage

```
dotnet add package minid
```

You can then use the provided `Id` struct to generate identifiers in your applications. 

```c#
public class Customer
{
    public Id Id { get; }

    public Customer()
    {
        Id = Id.NewId();
    }
}
```

To get the Base32-encoded value call `ToString()` on the `Id` value:

```c#
string encoded = customer.Id.ToString();
```

You can also initialise the `Id` type from an existing Guid:

```c#
var existingId = new Id(existingGuid);
```

To parse an encoded value:

```c#
if (Id.TryParse(encodedValue, out Id id))
{

}
```

### Prefixes

A common pattern is to prefix API resource identifiers to indicate the resource type, for example `cus_473cr1y0ghbyc3m1yfbwvn3nxx` to represent a customer identifier. This is particularly useful when an API needs to accept identifiers for multiple resource types so that it can handle the request accordingly.

To provide a prefix when generating a new `Id`:

```c#
var prefixedId = Id.NewId(prefix: "cus");
```

When specifying a prefix, the format of the encoded `Id` is `{prefix}_{encoded_guid_value}`.

To parse an encoded value you can optionally specify the prefix you expect. This is slightly more performant since you can benefit from defining your prefixes as a constant, avoiding an allocation:

```c#
const string CustomerPrefix = "cus";
if (Id.TryParse(encodedValue, CustomerPrefix, out Id id))
{

}
```

If you don't provide a prefix and one exists, it will be detected by the presence of the `_` separator, but no prefix validation will be performed. 

The implicit detection is required when needing to convert the values using a `TypeConverter` such as serializing and deserializing JSON (see below).

**Note that currently the prefix is not considered when comparing two `Id` values.**

### Serialization and Model-binding

A `TypeConverter` and System.Text.Json `JsonConverter` are included so that you can bind, serialize and deserialize `Id` values without explicit conversion.

Note that support for Type Converters was only added to [Newtonsoft Json.NET](https://www.newtonsoft.com/json) from version 10.

### Why a dedicated type?

We originally started to use Base32-encoding to format our API resource identifiers in responses. Internally we used the underlying Guid value. This became problematic when correlating client and internal requests. Effectively we had introduced a second implicit identifier and our codebase was littered with conversion to/from the encoded values. 

In our case it was far better to use the same encoded representation throughout our platform. Given we were mostly using document/no-sql databases which typically store Guid values as strings, we benefited from the reduction in size.

If you don't want to depend on this type you can use `string` but this will allocate more memory. My recommendation is to only convert to `string` when you need to.

## Benchmarks

```
BenchmarkDotNet=v0.13.2, OS=macOS Monterey 12.5 (21G72) [Darwin 21.6.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK=6.0.400
  [Host]     : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT AdvSIMD
```
|     Method |      Mean |    Error |   StdDev |   Gen0 | Allocated |
|----------- |----------:|---------:|---------:|-------:|----------:|
|      NewId | 113.48 ns | 0.659 ns | 0.584 ns |      - |         - |
| IdToString | 153.47 ns | 0.838 ns | 0.784 ns | 0.0381 |      80 B |
|    ParseId |  63.32 ns | 0.547 ns | 0.511 ns |      - |         - |

With prefixes:

|             Method |      Mean |    Error |   StdDev |   Gen0 | Allocated |
|------------------- |----------:|---------:|---------:|-------:|----------:|
|      NewPrefixedId | 111.78 ns | 0.437 ns | 0.387 ns |      - |         - |
| PrefixedIdToString | 172.68 ns | 1.133 ns | 1.060 ns | 0.0420 |      88 B |
|    ParsePrefixedId |  65.65 ns | 0.597 ns | 0.558 ns | 0.0153 |      32 B |
| ParseIdKnownPrefix |  55.82 ns | 0.266 ns | 0.249 ns |      - |         - |

Note that the `NewPrefixedId` benchmark makes uses of a constant for the prefix, hence the zero allocation.