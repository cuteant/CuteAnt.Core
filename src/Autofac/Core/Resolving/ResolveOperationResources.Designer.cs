﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Autofac.Core.Resolving {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ResolveOperationResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ResolveOperationResources() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Autofac.Core.Resolving.ResolveOperationResources", typeof(ResolveOperationResources).AssemblyX());
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   使用此强类型资源类，为所有资源查找
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 An exception was thrown while executing a resolve operation. See the InnerException for details. 的本地化字符串。
        /// </summary>
        internal static string ExceptionDuringResolve {
            get {
                return ResourceManager.GetString("ExceptionDuringResolve", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Probable circular dependency between factory-scoped components. Chain includes &apos;{0}&apos; 的本地化字符串。
        /// </summary>
        internal static string MaxDepthExceeded {
            get {
                return ResourceManager.GetString("MaxDepthExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 This resolve operation has already ended. When registering components using lambdas, the IComponentContext &apos;c&apos; parameter to the lambda cannot be stored. Instead, either resolve IComponentContext again from &apos;c&apos;, or resolve a Func&lt;&gt; based factory to create subsequent components from. 的本地化字符串。
        /// </summary>
        internal static string TemporaryContextDisposed {
            get {
                return ResourceManager.GetString("TemporaryContextDisposed", resourceCulture);
            }
        }
    }
}
