``` ini

BenchmarkDotNet=v0.10.3.0, OS=OSX
Processor=Intel(R) Core(TM) i7-4770HQ CPU 2.20GHz, ProcessorCount=8
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.1
  [Host] : .NET Core 4.6.25009.03, 64bit RyuJIT
  Core   : .NET Core 4.6.25009.03, 64bit RyuJIT

Job=Core  Runtime=Core  

```
 |          Method |      Mean |    StdErr |    StdDev |
 |---------------- |---------- |---------- |---------- |
 |       ClrBytes2 | 7.1742 ns | 0.0880 ns | 0.4900 ns |
 |    UnsafeBytes2 | 9.1925 ns | 0.1082 ns | 0.4589 ns |
 | BitBytesSergey2 | 1.5472 ns | 0.0195 ns | 0.0757 ns |
 |       ClrBytes4 | 7.3358 ns | 0.0496 ns | 0.1920 ns |
 |    UnsafeBytes4 | 7.6596 ns | 0.0491 ns | 0.1900 ns |
 | BitBytesSergey4 | 2.9156 ns | 0.0129 ns | 0.0500 ns |
 |       ClrBytes8 | 9.6935 ns | 0.0600 ns | 0.2325 ns |
 |    UnsafeBytes8 | 9.6479 ns | 0.0472 ns | 0.1827 ns |
 | BitBytesSergey8 | 6.7336 ns | 0.0298 ns | 0.1153 ns |
