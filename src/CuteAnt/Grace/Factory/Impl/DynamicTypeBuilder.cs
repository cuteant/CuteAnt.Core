using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CuteAnt;
using Grace.Data;
using Grace.DependencyInjection;
using Grace.Utilities;

namespace Grace.Factory.Impl
{
  /// <summary>Dynamic proxy type builder</summary>
  public class DynamicTypeBuilder
  {
    private static readonly object BuilderLock = new object();
    private static ModuleBuilder _moduleBuilder;
    private static int _proxyCount = 0;
#if NET40
    private static MethodInfo CloneMethod = typeof(IInjectionContext).GetMethod("Clone", new Type[0]);
    private static MethodInfo DelegateInvoke = typeof(ActivationStrategyDelegate).GetMethod("Invoke",
        new[] { typeof(IExportLocatorScope), typeof(IDisposalScope), typeof(IInjectionContext) });
    private static MethodInfo SetExtraDataMethod =
        typeof(IExtraDataContainer).GetMethod("SetExtraData", new[] { typeof(object), typeof(object), typeof(bool) });
#else
    private static MethodInfo CloneMethod = typeof(IInjectionContext).GetRuntimeMethod("Clone", new Type[0]);

    private static MethodInfo DelegateInvoke = typeof(ActivationStrategyDelegate).GetRuntimeMethod("Invoke",
        new[] { typeof(IExportLocatorScope), typeof(IDisposalScope), typeof(IInjectionContext) });

    private static MethodInfo SetExtraDataMethod =
        typeof(IExtraDataContainer).GetRuntimeMethod("SetExtraData", new[] { typeof(object), typeof(object), typeof(bool) });

#endif

    /// <summary>Default constructor</summary>
    public DynamicTypeBuilder()
    {
      SetupAssembly();
    }

    public class DelegateInfo
    {
      public DelegateInfo(MethodInfo method, List<DelegateParameterInfo> parameterInfos, FieldBuilder activation)
      {
        Method = method;
        ParameterInfos = parameterInfos;
        Activation = activation;
      }

      public MethodInfo Method { get; }

      public List<DelegateParameterInfo> ParameterInfos { get; }

      public FieldBuilder Activation { get; }
    }

    public class DelegateParameterInfo
    {
      public DelegateParameterInfo(ParameterInfo parameterInfo)
      {
        ParameterInfo = parameterInfo;
        UniqueId = UniqueStringId.Generate();
      }

      public string UniqueId { get; }

      public ParameterInfo ParameterInfo { get; }
    }

    /// <summary>Create new proxy type that implements the interface</summary>
    /// <param name="interfaceType">interface type</param>
    /// <param name="methods">delegates that needed for constructor</param>
    /// <returns></returns>
    public Type CreateType(Type interfaceType, out List<DelegateInfo> methods)
    {
      lock (BuilderLock)
      {
        _proxyCount++;

        var proxyBuilder = _moduleBuilder.DefineType(interfaceType.Name + "Proxy" + _proxyCount,
            TypeAttributes.Class | TypeAttributes.Public);

        proxyBuilder.AddInterfaceImplementation(interfaceType);

        var scopeField = proxyBuilder.DefineField("_scope", typeof(IExportLocatorScope),
            FieldAttributes.Private);

        var disposalScopeField = proxyBuilder.DefineField("_disposalScope", typeof(IDisposalScope),
            FieldAttributes.Private);

        var contextField = proxyBuilder.DefineField("_context", typeof(IInjectionContext),
            FieldAttributes.Private);

        int methodCount = 1;

        methods = new List<DelegateInfo>();

#if NET40
        foreach (var method in interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
#else
        foreach (var method in interfaceType.GetRuntimeMethods())
#endif
        {
          methods.Add(CreateMethod(method, proxyBuilder, methodCount, scopeField, disposalScopeField, contextField));

          methodCount++;
        }

        CreateConstructor(proxyBuilder, methods, scopeField, disposalScopeField, contextField);

#if NET40
        return proxyBuilder.CreateType();
#else
        return proxyBuilder.CreateTypeInfo().AsType();
#endif
      }
    }

    private void CreateConstructor(TypeBuilder proxyBuilder, List<DelegateInfo> methods, FieldBuilder scopeField, FieldBuilder disposalScopeField, FieldBuilder contextField)
    {
      var constructorParameterTypes = new List<Type> { typeof(IExportLocatorScope), typeof(IDisposalScope), typeof(IInjectionContext) };

      foreach (var method in methods)
      {
        constructorParameterTypes.Add(typeof(ActivationStrategyDelegate));
      }

      var proxyConstructor = proxyBuilder.DefineConstructor(MethodAttributes.Public,
          CallingConventions.Standard, constructorParameterTypes.ToArray());

      for (int i = 0; i < constructorParameterTypes.Count; i++)
      {
        proxyConstructor.DefineParameter(i + 1, ParameterAttributes.None, "arg" + (i + 1));
      }

      var proxyConstructorILGenerator = proxyConstructor.GetILGenerator();

      proxyConstructorILGenerator.Emit(OpCodes.Ldarg_0);
      proxyConstructorILGenerator.Emit(OpCodes.Ldarg_1);
      proxyConstructorILGenerator.Emit(OpCodes.Stfld, scopeField);

      proxyConstructorILGenerator.Emit(OpCodes.Ldarg_0);
      proxyConstructorILGenerator.Emit(OpCodes.Ldarg_2);
      proxyConstructorILGenerator.Emit(OpCodes.Stfld, disposalScopeField);

      proxyConstructorILGenerator.Emit(OpCodes.Ldarg_0);
      proxyConstructorILGenerator.Emit(OpCodes.Ldarg_3);
      proxyConstructorILGenerator.Emit(OpCodes.Stfld, contextField);

      int argIndex = 4;
      foreach (var method in methods)
      {
        proxyConstructorILGenerator.Emit(OpCodes.Ldarg_0);
        proxyConstructorILGenerator.Emit(OpCodes.Ldarg_S, (byte)argIndex);
        proxyConstructorILGenerator.Emit(OpCodes.Stfld, method.Activation);

        argIndex++;
      }

      proxyConstructorILGenerator.Emit(OpCodes.Ret);
    }

    private DelegateInfo CreateMethod(MethodInfo method, TypeBuilder proxyBuilder, int methodCount, FieldBuilder scopeField, FieldBuilder disposalScopeField, FieldBuilder contextField)
    {
      var parameters = method.GetParameters();

      var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;

      var methodBuilder = proxyBuilder.DefineMethod(method.Name, methodAttributes, method.ReturnType, parameters.Select(x => x.ParameterType).ToArray());

      var ilGenerator = methodBuilder.GetILGenerator();

      var activation = proxyBuilder.DefineField("_activationMethod" + methodCount, typeof(ActivationStrategyDelegate),
          FieldAttributes.Private);

      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, activation);

      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, scopeField);

      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, disposalScopeField);

      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldfld, contextField);
      ilGenerator.Emit(OpCodes.Callvirt, CloneMethod);

      var parameterInfos = new List<DelegateParameterInfo>();

      foreach (var parameter in parameters)
      {
        ilGenerator.Emit(OpCodes.Dup);

        var delegateParameterInfo = new DelegateParameterInfo(parameter);

        parameterInfos.Add(delegateParameterInfo);

        ilGenerator.Emit(OpCodes.Ldstr, delegateParameterInfo.UniqueId);

        switch (parameter.Position)
        {
          case 0:
            ilGenerator.Emit(OpCodes.Ldarg_1);
            break;

          case 1:
            ilGenerator.Emit(OpCodes.Ldarg_2);
            break;

          case 2:
            ilGenerator.Emit(OpCodes.Ldarg_3);
            break;

          default:
            ilGenerator.Emit(OpCodes.Ldarg_S, (byte)(parameter.Position + 1));
            break;
        }

        ilGenerator.Emit(OpCodes.Box, parameter.ParameterType);
        ilGenerator.Emit(OpCodes.Ldc_I4_1);

        ilGenerator.Emit(OpCodes.Callvirt, SetExtraDataMethod);

        ilGenerator.Emit(OpCodes.Pop);
      }

      ilGenerator.Emit(OpCodes.Callvirt, DelegateInvoke);

      if (method.ReturnType != null || method.ReturnType != TypeConstants.VoidType)
      {
#if NET40
        ilGenerator.Emit(method.ReturnType.IsValueType ?
#else
        ilGenerator.Emit(method.ReturnType.GetTypeInfo().IsValueType ?
#endif
                    OpCodes.Unbox_Any : OpCodes.Castclass, method.ReturnType);
      }

      ilGenerator.Emit(OpCodes.Ret);

      proxyBuilder.DefineMethodOverride(methodBuilder, method);

      return new DelegateInfo(method, parameterInfos, activation);
    }

    private void SetupAssembly()
    {
      if (_moduleBuilder == null)
      {
        lock (BuilderLock)
        {
          if (_moduleBuilder == null)
          {
#if NET40
            var dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
#else
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
#endif

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("DynamicProxyTypes");
          }
        }
      }
    }
  }
}