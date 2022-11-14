``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.2130)
Intel Core i5-8250U CPU 1.60GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.100
  [Host]        : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  .NET 6.0      : .NET 6.0.11 (6.0.1122.52304), X64 RyuJIT AVX2
  .NET 7.0      : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  NativeAOT 7.0 : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2


```
| Method |           Job |       Runtime |      Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|------- |-------------- |-------------- |----------:|---------:|---------:|------:|----------:|------------:|
| Simple |      .NET 6.0 |      .NET 6.0 | 109.85 ns | 1.389 ns | 1.160 ns |  1.00 |         - |          NA |
|  Smart |      .NET 6.0 |      .NET 6.0 |  61.86 ns | 0.783 ns | 0.654 ns |  0.56 |         - |          NA |
| Simple |      .NET 7.0 |      .NET 7.0 |  42.56 ns | 0.807 ns | 1.232 ns |  0.39 |         - |          NA |
|  Smart |      .NET 7.0 |      .NET 7.0 |  45.41 ns | 0.281 ns | 0.220 ns |  0.41 |         - |          NA |
| Simple | NativeAOT 7.0 | NativeAOT 7.0 |  51.96 ns | 0.272 ns | 0.241 ns |  0.47 |         - |          NA |
|  Smart | NativeAOT 7.0 | NativeAOT 7.0 |  47.71 ns | 0.157 ns | 0.131 ns |  0.43 |         - |          NA |
