using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET40
[assembly: AssemblyTitle("CuteAnt.Extensions.DependencyInjection for .NetFx4.0")]
#elif NET451 || DNX451
[assembly: AssemblyTitle("CuteAnt.Extensions.DependencyInjection for .NetFx4.5")]
#endif
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("CuteAnt.Extensions.DependencyInjection Library (Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("CuteAnt.Extensions.DependencyInjection Library (Flavor=Retail)")]
#endif

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("afc10671-736d-42fc-8799-cc6c63f6999e")]
