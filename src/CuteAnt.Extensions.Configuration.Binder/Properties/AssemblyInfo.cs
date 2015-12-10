using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET40
[assembly: AssemblyTitle("CuteAnt.Extensions.Configuration.Binder for .NetFx4.0")]
#elif NET451 || DNX451
[assembly: AssemblyTitle("CuteAnt.Extensions.Configuration.Binder for .NetFx4.5")]
#endif
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("CuteAnt.Extensions.Configuration.Binder Library (Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("CuteAnt.Extensions.Configuration.Binder Library (Flavor=Retail)")]
#endif

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("bdc0c2c7-e243-4629-9b5d-56b8de9330ed")]
