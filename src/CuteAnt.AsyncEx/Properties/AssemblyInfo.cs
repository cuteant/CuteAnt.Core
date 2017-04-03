using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("CuteAnt.AsyncEx")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("CuteAnt.AsyncEx Library (Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("CuteAnt.AsyncEx Library (Flavor=Retail)")]
#endif

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6f58e510-7903-43d5-918e-077491b8d8d8")]

[assembly: InternalsVisibleTo("CuteAnt.AsyncEx.Tests" + CuteAnt.AssemblyInfo.PublicKeyString)]
