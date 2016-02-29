﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


#if NET40
[assembly: AssemblyTitle("CuteAnt.Extensions.Logging.Serilog for .NetFx4.0")]
#elif NET451 || DNX451
[assembly: AssemblyTitle("CuteAnt.Extensions.Logging.Serilog for .NetFx4.5")]
#endif
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("CuteAnt.Extensions.Logging.Serilog Library (Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("CuteAnt.Extensions.Logging.Serilog Library (Flavor=Retail)")]
#endif

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("60c38815-6fb7-4c2d-bdc3-30dbc58206f4")]