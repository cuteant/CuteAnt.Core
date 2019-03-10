using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public delegate object EmptyCtorInvoker();

    partial class ReflectUtils
    {
        private const string c_ctorInvokerName = "CI<>";

        #region -- CreateInstance --

        /// <summary>Creates a new instance from the default constructor of type</summary>
        [Obsolete("=> ActivatorUtils.FastCreateInstance")]
        public static object CreateInstance(this Type instanceType)
        {
            if (instanceType == null) { return null; }

            return GetConstructorMethod(instanceType).Invoke();
        }

        /// <summary>Creates a new instance from the default constructor of type</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        [Obsolete("=> ActivatorUtils.FastCreateInstance")]
        public static T CreateInstance<T>(this Type instanceType)
        {
            if (instanceType == null) { return default(T); }

            return (T)GetConstructorMethod(instanceType).Invoke();
        }

        /// <summary>Creates a new instance from the default constructor of type</summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [Obsolete("=> ActivatorUtils.FastCreateInstance")]
        public static object CreateInstance(this string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) { return null; }

            if (!TypeUtils.TryResolveType(typeName, out var instanceType)) { return null; }

            return GetConstructorMethod(instanceType).Invoke();
        }

        #endregion

        #region -- GetConstructorMethod --

        private static readonly Func<Type, EmptyCtorInvoker> s_getConstructorMethodToCacheFunc = GetConstructorMethodToCache;
        private static CachedReadConcurrentDictionary<Type, EmptyCtorInvoker> s_constructorMethods =
            new CachedReadConcurrentDictionary<Type, EmptyCtorInvoker>(DictionaryCacheConstants.SIZE_MEDIUM);

        /// <summary>GetConstructorMethod</summary>
        /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        [MethodImpl(InlineMethod.Value)]
        public static EmptyCtorInvoker GetConstructorMethod(this Type instanceType)
            => s_constructorMethods.GetOrAdd(instanceType, s_getConstructorMethodToCacheFunc);

        /// <summary>GetConstructorMethodToCache</summary>
        /// <remarks>Code taken from ServiceStack.Text Library &lt;a href="https://github.com/ServiceStack/ServiceStack.Text"&gt;</remarks>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        private static EmptyCtorInvoker GetConstructorMethodToCache(Type instanceType)
        {
            if (instanceType == TypeConstants.StringType) { return () => string.Empty; }

            if (instanceType.IsInterface)
            {
                if (instanceType.HasGenericType())
                {
                    var genericType = instanceType.GetTypeWithGenericTypeDefinitionOfAny(typeof(IDictionary<,>));

                    if (genericType != null)
                    {
#if NET40
                        var keyType = genericType.GenericTypeArguments()[0];
                        var valueType = genericType.GenericTypeArguments()[1];
#else
                        var keyType = genericType.GenericTypeArguments[0];
                        var valueType = genericType.GenericTypeArguments[1];
#endif
                        return GetConstructorMethodToCache(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                    }

                    genericType = instanceType.GetTypeWithGenericTypeDefinitionOfAny(
                        typeof(IEnumerable<>),
                        typeof(ICollection<>),
                        typeof(IList<>));

                    if (genericType != null)
                    {
#if NET40
                        var elementType = genericType.GenericTypeArguments()[0];
#else
                        var elementType = genericType.GenericTypeArguments[0];
#endif
                        return GetConstructorMethodToCache(typeof(List<>).MakeGenericType(elementType));
                    }
                }
            }
            else if (instanceType.IsArray)
            {
                return () => Array.CreateInstance(instanceType.GetElementType(), 0);
            }
            else if (instanceType.IsGenericTypeDefinition)
            {
                var genericArgs = instanceType.GetGenericArguments();
                var typeArgs = new Type[genericArgs.Length];
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    typeArgs[i] = TypeConstants.ObjectType;
                }

                var realizedType = instanceType.MakeGenericType(typeArgs);

#pragma warning disable 0618
                return realizedType.CreateInstance;
#pragma warning restore 0618
            }

            var emptyCtor = instanceType.GetEmptyConstructor();
            if (emptyCtor != null)
            {
                var dm = new DynamicMethod("MyCtor", instanceType, Type.EmptyTypes, typeof(ReflectUtils).Module, true);
                var ilgen = dm.GetILGenerator();
                ilgen.Emit(OpCodes.Nop);
                ilgen.Emit(OpCodes.Newobj, emptyCtor);
                ilgen.Emit(OpCodes.Ret);

                return (EmptyCtorInvoker)dm.CreateDelegate(typeof(EmptyCtorInvoker));
            }

            //Anonymous types don't have empty constructors
            return () => FormatterServices.GetUninitializedObject(instanceType);
            //return () => FormatterServices.GetSafeUninitializedObject(instanceType);
        }

        #endregion

        #region -- MakeDelegateForCtor --

        /// <summary>Generates or gets a weakly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
        public static CtorInvoker<object> MakeDelegateForCtor(this Type instanceType, params Type[] ctorParamTypes)
            => MakeDelegateForCtor<object>(instanceType, ctorParamTypes);

        /// <summary>Generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
        public static CtorInvoker<T> MakeDelegateForCtor<T>(this Type instanceType, params Type[] paramTypes)
        {
            if (!TryMakeDelegateForCtor<T>(instanceType, paramTypes, out var result))
            {
                GetTypeAccessException(instanceType, paramTypes);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void GetTypeAccessException(Type instanceType, Type[] paramTypes)
        {
            throw GetTypeAccessException();
            TypeAccessException GetTypeAccessException()
            {
                return new TypeAccessException("Generating constructor for type: " + instanceType +
                  (paramTypes == null || paramTypes.Length == 0 ? " No empty constructor found!" :
                  " No constructor found that matches the following parameter types: " +
                  string.Join(",", paramTypes.Select(x => x.Name).ToArray())));

            }
        }

        /// <summary>Try generates or gets a weakly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
        public static bool TryMakeDelegateForCtor(this Type instanceType, Type[] paramTypes, out CtorInvoker<object> result)
            => TryMakeDelegateForCtor<object>(instanceType, paramTypes, out result);

        /// <summary>Try generates or gets a strongly-typed open-instance delegate to the specified type constructor that takes the specified type params.</summary>
        public static bool TryMakeDelegateForCtor<T>(this Type instanceType, Type[] paramTypes, out CtorInvoker<T> result)
        {
            if (null == instanceType) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.instanceType);

            ConstructorInfo ctor = null;
            if (!(instanceType == TypeConstants.StringType ||
                instanceType.IsArray || instanceType.IsInterface || instanceType.IsGenericTypeDefinition))
            {
                try
                {
                    ctor = instanceType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
                        null, paramTypes ?? Type.EmptyTypes, null);
                }
                catch { }
            }
            if (ctor == null)
            {
                if (paramTypes == null || paramTypes.Length == 0)
                {
                    var emptyInvoker = instanceType.GetConstructorMethod();
                    result = (object[] ps) => (T)emptyInvoker.Invoke();
                    return true;
                }
                result = null; return false;
            }

            result = MakeDelegateForCtor<T>(instanceType, paramTypes, ctor);
            return true;
        }

        internal static CtorInvoker<T> MakeDelegateForCtor<T>(Type instanceType, Type[] paramTypes, ConstructorInfo ctor)
        {
            if (null == paramTypes) { paramTypes = Type.EmptyTypes; }

            var dynMethod = new DynamicMethod(c_ctorInvokerName,
                MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard,
                typeof(T), new Type[] { TypeConstants.ObjectArrayType }, instanceType, true);

            var il = dynMethod.GetILGenerator();
            GenCtor<T>(instanceType, il, paramTypes, ctor);

            return (CtorInvoker<T>)dynMethod.CreateDelegate(typeof(CtorInvoker<T>));
        }

        private static void GenCtor<T>(Type instanceType, ILGenerator il, Type[] paramTypes, ConstructorInfo ctor)
        {
            // arg0: object[] arguments
            // goal: return new T(arguments)
            Type targetType = typeof(T) == TypeConstants.ObjectType ? instanceType : typeof(T);

            if (targetType.IsValueType && paramTypes.Length == 0)
            {
                var tmp = il.DeclareLocal(targetType);
                il.Emit(OpCodes.Ldloca, tmp);
                il.Emit(OpCodes.Initobj, targetType);
                il.Emit(OpCodes.Ldloc, 0);
            }
            else
            {
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

            if (typeof(T) == TypeConstants.ObjectType && targetType.IsValueType)
            {
                il.Emit(OpCodes.Box, targetType);
            }

            il.Emit(OpCodes.Ret);
        }

        #endregion
    }
}