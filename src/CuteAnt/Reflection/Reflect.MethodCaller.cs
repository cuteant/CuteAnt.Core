using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
  public delegate TReturn MethodCaller<TTarget, TReturn>(TTarget target, object[] args);

  partial class ReflectUtils
  {
    private const string kMethodCallerName = "MC<>";

    /// <summary>Generates a strongly-typed open-instance delegate to invoke the specified method</summary>
    public static MethodCaller<TTarget, TReturn> MakeDelegateForCall<TTarget, TReturn>(this MethodInfo method)
    {
      return GenDelegateForMember<MethodCaller<TTarget, TReturn>, MethodInfo>(
          method, kMethodCallerName, GenMethodInvocation<TTarget>,
          typeof(TReturn), typeof(TTarget), typeof(object[]));
    }

    /// <summary>Generates a weakly-typed open-instance delegate to invoke the specified method.</summary>
    public static MethodCaller<object, object> MakeDelegateForCall(this MethodInfo method)
        => MakeDelegateForCall<object, object>(method);

    /// <summary>Executes the delegate on the specified target and arguments but only if it's not null.</summary>
    public static void SafeInvoke<TTarget, TValue>(this MethodCaller<TTarget, TValue> caller, TTarget target, params object[] args)
        => caller?.Invoke(target, args);

    private static TDelegate GenDelegateForMember<TDelegate, TMember>(TMember member, string dynMethodName,
      Action<TMember, ILGenerator> generator, Type returnType, params Type[] paramTypes)
      where TMember : MemberInfo where TDelegate : class
    {
      var dynMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, true);

      var il = dynMethod.GetILGenerator();
      generator(member, il);

      var result = dynMethod.CreateDelegate(typeof(TDelegate));
      return (TDelegate)(object)result;
    }

    private static void GenMethodInvocation<TTarget>(MethodInfo method, ILGenerator il)
    {
      var weaklyTyped = typeof(TTarget) == typeof(object);

      // push target if not static (instance-method. in that case first arg is always 'this')
      if (!method.IsStatic)
      {
        var targetType = weaklyTyped ? method.DeclaringType : typeof(TTarget);
        il.DeclareLocal(targetType);
        il.Emit(OpCodes.Ldarg_0);
        if (weaklyTyped)
        {
          il.Emit(OpCodes.Unbox_Any, targetType);
        }
        il.Emit(OpCodes.Stloc_0);
        if (targetType.GetTypeInfo().IsValueType)
        {
          il.Emit(OpCodes.Ldloca, 0);
        }
        else
        {
          il.Emit(OpCodes.Ldloc, 0);
        }
      }

      // push arguments in order to call method
      var prams = method.GetParameters();
      for (int i = 0, imax = prams.Length; i < imax; i++)
      {
        il.Emit(OpCodes.Ldarg_1);     // push array
        il.Emit(OpCodes.Ldc_I4, i);   // push index
        il.Emit(OpCodes.Ldelem_Ref);  // pop array, index and push array[index]

        var param = prams[i];
        var paramType = param.ParameterType;

        var dataType = paramType.GetTypeInfo().IsByRef ? paramType.GetElementType() : paramType;

        var tmp = il.DeclareLocal(dataType);
        il.Emit(OpCodes.Unbox_Any, dataType);
        il.Emit(OpCodes.Stloc, tmp);
        if (paramType.IsByRef)
        {
          il.Emit(OpCodes.Ldloca, tmp);
        }
        else
        {
          il.Emit(OpCodes.Ldloc, tmp);
        }
      }

      // perform the correct call (pushes the result)
      if (method.IsStatic || method.IsFinal)
      {
        il.Emit(OpCodes.Call, method);
      }
      else
      {
        il.Emit(OpCodes.Callvirt, method);
      }

      // if method wasn't static that means we declared a temp local to load the target
      // that means our local variables index for the arguments start from 1
      int localVarStart = method.IsStatic ? 0 : 1;
      for (int i = 0; i < prams.Length; i++)
      {
        var paramType = prams[i].ParameterType;
        if (paramType.IsByRef)
        {
          var byRefType = paramType.GetElementType();
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldc_I4, i);
          il.Emit(OpCodes.Ldloc, i + localVarStart);
          if (byRefType.GetTypeInfo().IsValueType)
          {
            il.Emit(OpCodes.Box, byRefType);
          }
          il.Emit(OpCodes.Stelem_Ref);
        }
      }

      if (method.ReturnType == typeof(void))
      {
        il.Emit(OpCodes.Ldnull);
      }
      else if (weaklyTyped)
      {
        var returnType = method.ReturnType;
        if (returnType.IsValueType) { il.Emit(OpCodes.Box, returnType); }
      }

      il.Emit(OpCodes.Ret);
    }
  }
}