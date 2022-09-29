``` ini

BenchmarkDotNet=v0.13.2, OS=macOS Monterey 12.5 (21G72) [Darwin 21.6.0]
Apple M1 Max, 1 CPU, 10 logical and 10 physical cores
.NET SDK=6.0.400
  [Host]     : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT AdvSIMD


```
|       Method |      Mean |    Error |   StdDev |   Gen0 | Allocated |
|------------- |----------:|---------:|---------:|-------:|----------:|
|        NewId | 112.24 ns | 0.430 ns | 0.381 ns |      - |         - |
|   IdToString | 151.08 ns | 0.632 ns | 0.528 ns | 0.0381 |      80 B |
| GuidToString | 217.03 ns | 0.606 ns | 0.537 ns | 0.0458 |      96 B |
|      ParseId |  52.88 ns | 0.235 ns | 0.220 ns |      - |         - |
