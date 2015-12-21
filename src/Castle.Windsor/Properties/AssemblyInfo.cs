using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET40
[assembly: AssemblyTitle("Castle.Windsor for .NETFramework v4.0")]
[assembly: AssemblyDescription("Castle.Windsor for .NETFramework v4.0")]
#elif NET451 || DNX451
[assembly: AssemblyTitle("Castle.Windsor for .NETFramework v4.5")]
[assembly: AssemblyDescription("Castle.Windsor for .NETFramework v4.5")]
#endif
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyCopyright("Copyright(c) 2004 - 2015 Castle Project - http://www.castleproject.org")]

[assembly: AssemblyVersion("3.3.0.0")]
[assembly: AssemblyFileVersion("3.3.0.0")]

[assembly: AssemblyCompany("Castle Project")]
[assembly: AssemblyProduct("Castle.Windsor")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyMetadata("Serviceable", "True")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a9d1aedc-ab82-491d-8553-d17da6de2bf1")]

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
