``` ini

BenchmarkDotNet=v0.10.3.0, OS=OSX
Processor=Intel(R) Core(TM) i7-4770HQ CPU 2.20GHz, ProcessorCount=8
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.1
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.25009.03, 64bit RyuJIT


```
 | Method |           Mean |      StdDev |
 |------- |--------------- |------------ |
 |   Fast |     17.7902 ns |   0.3832 ns |
 |   Old2 | 14,193.0784 ns | 333.0168 ns |
