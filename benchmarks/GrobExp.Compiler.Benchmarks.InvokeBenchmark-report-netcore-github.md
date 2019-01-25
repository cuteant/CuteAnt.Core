``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.523 (1803/April2018Update/Redstone4)
Intel Core i5-3450 CPU 3.10GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=3027372 Hz, Resolution=330.3195 ns, Timer=TSC
.NET Core SDK=2.2.102
  [Host]     : .NET Core 2.2.1 (CoreCLR 4.6.27207.03, CoreFX 4.6.27207.03), 64bit RyuJIT
  DefaultJob : .NET Core 2.2.1 (CoreCLR 4.6.27207.03, CoreFX 4.6.27207.03), 64bit RyuJIT


```
|                                   Method |         Mean |      Error |     StdDev |
|----------------------------------------- |-------------:|-----------:|-----------:|
|                           Simple_Compile |     2.987 ns |  0.0101 ns |  0.0095 ns |
|     Simple_GroboCompile_Without_Checking |     8.906 ns |  0.0348 ns |  0.0326 ns |
|        Simple_GroboCompile_With_Checking |    10.520 ns |  0.0257 ns |  0.0228 ns |
|                       SubLambda1_Compile | 1,041.564 ns |  5.5762 ns |  5.2160 ns |
| SubLambda1_GroboCompile_Without_Checking |    55.241 ns |  0.1556 ns |  0.1455 ns |
|    SubLambda1_GroboCompile_With_Checking |    58.039 ns |  0.3293 ns |  0.3080 ns |
|                       SubLambda2_Compile | 4,136.405 ns | 18.8414 ns | 17.6243 ns |
| SubLambda2_GroboCompile_Without_Checking |   260.487 ns |  1.4624 ns |  1.2212 ns |
|    SubLambda2_GroboCompile_With_Checking |   282.262 ns |  1.4203 ns |  1.3285 ns |
|                          Invoke1_Compile |     4.386 ns |  0.0142 ns |  0.0126 ns |
|    Invoke1_GroboCompile_Without_Checking |    10.999 ns |  0.0264 ns |  0.0247 ns |
|       Invoke1_GroboCompile_With_Checking |    16.919 ns |  0.0500 ns |  0.0468 ns |
|                          Invoke2_Compile |     1.462 ns |  0.0052 ns |  0.0048 ns |
|    Invoke2_GroboCompile_Without_Checking |     2.936 ns |  0.0225 ns |  0.0210 ns |
|       Invoke2_GroboCompile_With_Checking |     4.440 ns |  0.0568 ns |  0.0531 ns |
|                          Invoke3_Compile |     1.493 ns |  0.0197 ns |  0.0184 ns |
|    Invoke3_GroboCompile_Without_Checking |     5.743 ns |  0.0392 ns |  0.0366 ns |
|       Invoke3_GroboCompile_With_Checking |     8.889 ns |  0.1190 ns |  0.1113 ns |
|                        Factorial_Compile |     4.158 ns |  0.0258 ns |  0.0229 ns |
|  Factorial_GroboCompile_Without_Checking |    10.375 ns |  0.0886 ns |  0.0828 ns |
|     Factorial_GroboCompile_With_Checking |    10.275 ns |  0.1489 ns |  0.1393 ns |
|                           Switch_Compile |     2.331 ns |  0.0138 ns |  0.0129 ns |
|     Switch_GroboCompile_Without_Checking |     4.645 ns |  0.0165 ns |  0.0155 ns |
|        Switch_GroboCompile_With_Checking |     4.675 ns |  0.0143 ns |  0.0134 ns |
