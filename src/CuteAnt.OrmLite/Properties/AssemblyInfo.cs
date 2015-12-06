using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
#if NET40
[assembly: AssemblyTitle("CuteAnt.OrmLite for .NetFx4.0")]
#elif NET451
[assembly: AssemblyTitle("CuteAnt.OrmLite for .NetFx4.5")]
#endif
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("数据映射框架(Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("数据映射框架(Flavor=Retail)")]
#endif

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("fdd6dfe8-d74f-4b35-b7d3-edf6327f0438")]
