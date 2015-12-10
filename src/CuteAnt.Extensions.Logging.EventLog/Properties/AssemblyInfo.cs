using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET40
[assembly: AssemblyTitle("CuteAnt.Extensions.Logging.EventLog for .NetFx4.0")]
#elif NET451 || DNX451
[assembly: AssemblyTitle("CuteAnt.Extensions.Logging.EventLog for .NetFx4.5")]
#endif
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("CuteAnt.Extensions.Logging.EventLog Library (Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("CuteAnt.Extensions.Logging.EventLog Library (Flavor=Retail)")]
#endif

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("49ff5b5f-1c42-40e2-a51e-cf9e489529ef")]
