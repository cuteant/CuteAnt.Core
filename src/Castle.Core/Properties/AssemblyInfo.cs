using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET40
[assembly: AssemblyTitle("Castle.Core for .NETFramework v4.0")]
[assembly: AssemblyDescription("Castle.Core for .NETFramework v4.0")]
#elif NET451 || DNX451
[assembly: AssemblyTitle("Castle.Core for .NETFramework v4.5")]
[assembly: AssemblyDescription("Castle.Core for .NETFramework v4.5")]
#endif
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyCopyright("Copyright(c) 2004 - 2015 Castle Project - http://www.castleproject.org")]

[assembly: AssemblyVersion("3.3.3.0")]
[assembly: AssemblyFileVersion("3.3.3.0")]

[assembly: AssemblyCompany("Castle Project")]
[assembly: AssemblyProduct("Castle.Core")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyMetadata("Serviceable", "True")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("85bfe95d-21a1-4956-8954-4631409bf902")]

#if NET40 || PORTABLE
namespace System.Reflection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    internal sealed class AssemblyMetadataAttribute : Attribute
    {
        public AssemblyMetadataAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
#endif
