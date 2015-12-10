
using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CuteAnt;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
#if NET40
[assembly: AssemblyTitle("CuteAnt for .NetFx4.0")]
#elif NET451 || DNX451
[assembly: AssemblyTitle("CuteAnt for .NetFx4.5")]
#elif NET46 || DNX46
[assembly: AssemblyTitle("CuteAnt for .NetFx4.6")]
#endif
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("Core Library (Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("Core Library (Flavor=Retail)")]
#endif
// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("DB46B13A-906F-426C-B84B-A840D9FF2433")]
