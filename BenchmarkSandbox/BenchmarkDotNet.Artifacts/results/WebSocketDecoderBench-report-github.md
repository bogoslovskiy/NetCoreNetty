``` ini

BenchmarkDotNet=v0.10.3.0, OS=OSX
Processor=Intel(R) Core(TM) i7-4770HQ CPU 2.20GHz, ProcessorCount=8
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.1
  [Host] : .NET Core 4.6.25009.03, 64bit RyuJIT [AttachedDebugger]
  Core   : .NET Core 4.6.25009.03, 64bit RyuJIT

Job=Core  Runtime=Core  

```
 |                  Method |       Mean |    StdDev |
 |------------------------ |----------- |---------- |
 |    DecodeFrameSimpleBuf | 89.7596 ns | 1.2523 ns |
 | DecodeFrameUnmanagedBuf | 89.5514 ns | 1.1080 ns |
