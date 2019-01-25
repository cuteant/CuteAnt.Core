``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.523 (1803/April2018Update/Redstone4)
Intel Core i5-3450 CPU 3.10GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=3027372 Hz, Resolution=330.3195 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3260.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3260.0


```
|                                   Method |          Mean |     Error |    StdDev |
|----------------------------------------- |--------------:|----------:|----------:|
|                           Simple_Compile |    30.8630 ns | 0.0676 ns | 0.0633 ns |
|     Simple_GroboCompile_Without_Checking |    11.2014 ns | 0.0249 ns | 0.0221 ns |
|        Simple_GroboCompile_With_Checking |    11.9458 ns | 0.0147 ns | 0.0137 ns |
|                       SubLambda1_Compile | 1,125.5403 ns | 2.2686 ns | 2.1221 ns |
| SubLambda1_GroboCompile_Without_Checking |    59.9492 ns | 0.2163 ns | 0.2023 ns |
|    SubLambda1_GroboCompile_With_Checking |    63.4800 ns | 0.3409 ns | 0.3189 ns |
|                       SubLambda2_Compile | 4,611.5082 ns | 9.2972 ns | 8.6966 ns |
| SubLambda2_GroboCompile_Without_Checking |   285.8264 ns | 1.0048 ns | 0.9399 ns |
|    SubLambda2_GroboCompile_With_Checking |   302.1326 ns | 1.2181 ns | 1.1395 ns |
|                          Invoke1_Compile |     2.3937 ns | 0.0112 ns | 0.0104 ns |
|    Invoke1_GroboCompile_Without_Checking |    10.8634 ns | 0.0182 ns | 0.0170 ns |
|       Invoke1_GroboCompile_With_Checking |    12.2780 ns | 0.0231 ns | 0.0193 ns |
|                          Invoke2_Compile |     0.8966 ns | 0.0062 ns | 0.0058 ns |
|    Invoke2_GroboCompile_Without_Checking |     2.4003 ns | 0.0116 ns | 0.0109 ns |
|       Invoke2_GroboCompile_With_Checking |     3.8883 ns | 0.0135 ns | 0.0127 ns |
|                          Invoke3_Compile |     1.1957 ns | 0.0075 ns | 0.0070 ns |
|    Invoke3_GroboCompile_Without_Checking |     5.1843 ns | 0.0126 ns | 0.0118 ns |
|       Invoke3_GroboCompile_With_Checking |     8.3517 ns | 0.0261 ns | 0.0244 ns |
|                        Factorial_Compile |     2.3925 ns | 0.0123 ns | 0.0115 ns |
|  Factorial_GroboCompile_Without_Checking |    12.9183 ns | 0.0203 ns | 0.0180 ns |
|     Factorial_GroboCompile_With_Checking |    12.9260 ns | 0.0168 ns | 0.0157 ns |
|                           Switch_Compile |     2.0919 ns | 0.0087 ns | 0.0082 ns |
|     Switch_GroboCompile_Without_Checking |     3.3878 ns | 0.0095 ns | 0.0089 ns |
|        Switch_GroboCompile_With_Checking |     3.3799 ns | 0.0129 ns | 0.0121 ns |
