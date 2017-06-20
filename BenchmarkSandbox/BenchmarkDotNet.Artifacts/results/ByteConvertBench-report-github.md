``` ini

BenchmarkDotNet=v0.10.3.0, OS=OSX
Processor=Intel(R) Core(TM) i7-4770HQ CPU 2.20GHz, ProcessorCount=8
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.1
  [Host] : .NET Core 4.6.25009.03, 64bit RyuJIT
  Core   : .NET Core 4.6.25009.03, 64bit RyuJIT

Job=Core  Runtime=Core  

```
 |            Method |      Mean |    StdErr |    StdDev |
 |------------------ |---------- |---------- |---------- |
 |   BitBytesSergey8 | 4.5761 ns | 0.0776 ns | 0.3007 ns |
 | BitBytesSergey8_2 | 4.6600 ns | 0.0655 ns | 0.2537 ns |
