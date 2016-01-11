using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("protobuf-net")]
[assembly: AssemblyDescription("Protocol Buffers for .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Marc Gravell")]
[assembly: AssemblyProduct("protobuf-net")]
[assembly: AssemblyCopyright("See http://code.google.com/p/protobuf-net/")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ff1be072-0048-4c41-9a6a-683a70214867")]

#if !PORTABLE
[assembly: ComVisible(false)]
#endif
[assembly: CLSCompliant(false)]

[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: NeutralResourcesLanguage("en-US")]

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