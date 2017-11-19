using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
  public delegate T CtorInvoker<T>(object[] parameters);
  /// <summary>EmptyCtorDelegate</summary>
  /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
  /// <returns></returns>
  public delegate object EmptyCtorDelegate();

  partial class ReflectUtils
  {
    private const string kCtorInvokerName = "CI<>";
    private static readonly DictionaryCache<Type, DictionaryCache<int, Delegate>> s_ctorInvokerCache =
        new DictionaryCache<Type, DictionaryCache<int, Delegate>>(DictionaryCacheConstants.SIZE_SMALL);

    #region -- CreateInstance --

    /// <summary>Creates a new instance from the default constructor of type</summary>
    public static object CreateInstance(this Type type)
    {
      if (type == null) { return null; }

      var ctorFn = GetConstructorMethod(type);
      return ctorFn();
    }

    /// <summary>Creates a new instance from the default constructor of type</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static T CreateInstance<T>(this Type type)
    {
      if (type == null) { return default(T); }

      var ctorFn = GetConstructorMethod(type);
      return (T)ctorFn();
    }

    /// <summary>Creates a new instance from the default constructor of type</summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static object CreateInstance(string typeName)
    {
      if (typeName == null) { return null; }

      if (!TypeUtils.TryResolveType(typeName, out var type)) { return null; }

      var ctorFn = GetConstructorMethod(type);
      return ctorFn();
    }

    #endregion

    #region -- GetConstructorMethod --

    private static readonly Func<Type, EmptyCtorDelegate> s_getConstructorMethodToCacheFunc = GetConstructorMethodToCache;
    private static CachedReadConcurrentDictionary<Type, EmptyCtorDelegate> s_constructorMethods =
        new CachedReadConcurrentDictionary<Type, EmptyCtorDelegate>(DictionaryCacheConstants.SIZE_MEDIUM);

    /// <summary>GetConstructorMethod</summary>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(InlineMethod.Value)]
    public static EmptyCtorDelegate GetConstructorMethod(Type type)
        => s_constructorMethods.GetOrAdd(type, s_getConstructorMethodToCacheFunc);

    /// <summary>GetConstructorMethodToCache</summary>
    /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
    /// <param name="type"></param>
    /// <returns></returns>
    private static EmptyCtorDelegate GetConstructorMethodToCache(Type type)
    {
      if (type == TypeConstants.StringType)
      {
        return () => string.Empty;
      }
      else if (type.IsInterface)
      {
        if (type.HasGenericType())
        {
          var genericType = type.GetTypeWithGenericTypeDefinitionOfAny(typeof(IDictionary<,>));

          if (genericType != null)
          {
            var keyType = genericType.GenericTypeArguments()[0];
            var valueType = genericType.GenericTypeArguments()[1];
            return GetConstructorMethodToCache(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
          }

          genericType = type.GetTypeWithGenericTypeDefinitionOfAny(
              typeof(IEnumerable<>),
              typeof(ICollection<>),
              typeof(IList<>));

          if (genericType != null)
          {
            var elementType = genericType.GenericTypeArguments()[0];
            return GetConstructorMethodToCache(typeof(List<>).MakeGenericType(elementType));
          }
        }
      }
      else if (type.IsArray)
      {
        return () => Array.CreateInstance(type.GetElementType(), 0);
      }
      else if (type.IsGenericTypeDefinition)
      {
        var genericArgs = type.GetGenericArguments();
        var typeArgs = new Type[genericArgs.Length];
        for (var i = 0; i < genericArgs.Length; i++)
          typeArgs[i] = typeof(object);

        var realizedType = type.MakeGenericType(typeArgs);

        return realizedType.CreateInstance;
      }

      var emptyCtor = type.GetEmptyConstructor();
      if (emptyCtor != null)
      {
        var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes, typeof(ReflectUtils).Module, true);
        var ilgen = dm.GetILGenerator();
        ilgen.Emit(System.Reflection.Emit.OpCodes.Nop);
        ilgen.Emit(System.Reflection.Emit.OpCodes.Newobj, emptyCtor);
        ilgen.Emit(System.Reflection.Emit.OpCodes.Ret);

        return (EmptyCtorDelegate)dm.CreateDelegate(typeof(EmptyCtorDelegate));
      }

      //Anonymous types don't have empty constructors
      return () => FormatterServices.GetUninitializedObject(type);
      // return FormatterServices.GetSafeUninitializedObject(Type);
    }

    #endregion

    #region -- MakeDelegateForCtor --

    /// <summary>Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
    public static CtorInvoker<T> MakeDelegateForCtor<T>(this Type type, params Type[] paramTypes)
    {
      int key = kCtorInvokerName.GetHashCode() ^ type.GetHashCode();
      for (int i = 0; i < paramTypes.Length; i++)
      {
        key ^= paramTypes[i].GetHashCode();
      }

      var cache = s_ctorInvokerCache.GetItem(type, k => new DictionaryCache<int, Delegate>());
      var result = cache.GetItem(key, k =>
      {
        var dynMethod = new DynamicMethod(kCtorInvokerName, typeof(T), new Type[] { typeof(object[]) });

        var il = dynMethod.GetILGenerator();
        GenCtor<T>(type, il, paramTypes);

        return dynMethod.CreateDelegate(typeof(CtorInvoker<T>));
      });
      return (CtorInvoker<T>)result;
    }

    /// <summary>Generates or gets a weakly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
    public static CtorInvoker<object> MakeDelegateForCtor(this Type type, params Type[] ctorParamTypes)
        => MakeDelegateForCtor<object>(type, ctorParamTypes);

    private static void GenCtor<T>(Type type, ILGenerator il, Type[] paramTypes)
    {
      // arg0: object[] arguments
      // goal: return new T(arguments)
      Type targetType = typeof(T) == typeof(object) ? type : typeof(T);

      if (targetType.IsValueType && paramTypes.Length == 0)
      {
        var tmp = il.DeclareLocal(targetType);
        il.Emit(OpCodes.Ldloca, tmp);
        il.Emit(OpCodes.Initobj, targetType);
        il.Emit(OpCodes.Ldloc, 0);
      }
      else
      {
        var ctor = targetType.GetConstructor(paramTypes);
        if (ctor == null)
        {
          throw new Exception("Generating constructor for type: " + targetType +
              (paramTypes.Length == 0 ? "No empty constructor found!" :
              "No constructor found that matches the following parameter types: " +
              string.Join(",", paramTypes.Select(x => x.Name).ToArray())));
        }

        // push parameters in order to then call ctor
        for (int i = 0, imax = paramTypes.Length; i < imax; i++)
        {
          il.Emit(OpCodes.Ldarg_0);                   // push args array
          il.Emit(OpCodes.Ldc_I4, i);                 // push index
          il.Emit(OpCodes.Ldelem_Ref);                // push array[index]
          il.Emit(OpCodes.Unbox_Any, paramTypes[i]);  // cast
        }

        il.Emit(OpCodes.Newobj, ctor);
      }

      if (typeof(T) == typeof(object) && targetType.IsValueType)
      {
        il.Emit(OpCodes.Box, targetType);
      }

      il.Emit(OpCodes.Ret);
    }

    #endregion
  }
}