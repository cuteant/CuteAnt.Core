/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CuteAnt.Reflection
{
  /// <summary>动态生成代码Emit助手。仅提供扩展功能，不封装基本功能</summary>
  public static class EmitHelper
  {
    #region -- 方法 --

    /// <summary>基于Ldc_I4指令的整数推送，自动选择最合适的指令</summary>
    /// <param name="IL">指令</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static ILGenerator Ldc_I4(this ILGenerator IL, Int32 value)
    {
      switch (value)
      {
        case -1:
          IL.Emit(OpCodes.Ldc_I4_M1);
          return IL;

        case 0:
          IL.Emit(OpCodes.Ldc_I4_0);
          return IL;

        case 1:
          IL.Emit(OpCodes.Ldc_I4_1);
          return IL;

        case 2:
          IL.Emit(OpCodes.Ldc_I4_2);
          return IL;

        case 3:
          IL.Emit(OpCodes.Ldc_I4_3);
          return IL;

        case 4:
          IL.Emit(OpCodes.Ldc_I4_4);
          return IL;

        case 5:
          IL.Emit(OpCodes.Ldc_I4_5);
          return IL;

        case 6:
          IL.Emit(OpCodes.Ldc_I4_6);
          return IL;

        case 7:
          IL.Emit(OpCodes.Ldc_I4_7);
          return IL;

        case 8:
          IL.Emit(OpCodes.Ldc_I4_8);
          return IL;
      }
      if (value > -129 && value < 128)
        IL.Emit(OpCodes.Ldc_I4_S, (SByte)value);
      else
        IL.Emit(OpCodes.Ldc_I4, value);
      return IL;
    }

    /// <summary>基于Ldarg指令的参数加载，自动选择最合适的指令</summary>
    /// <param name="IL">指令</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static ILGenerator Ldarg(this ILGenerator IL, Int32 value)
    {
      switch (value)
      {
        case 0:
          IL.Emit(OpCodes.Ldarg_0);
          return IL;

        case 1:
          IL.Emit(OpCodes.Ldarg_1);
          return IL;

        case 2:
          IL.Emit(OpCodes.Ldarg_2);
          return IL;

        case 3:
          IL.Emit(OpCodes.Ldarg_3);
          return IL;
        default:
          IL.Emit(OpCodes.Ldarg, value);
          return IL;
      }
    }

    /// <summary>基于Stloc指令的弹栈，自动选择最合适的指令</summary>
    /// <param name="IL">指令</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static ILGenerator Stloc(this ILGenerator IL, Int32 value)
    {
      switch (value)
      {
        case 0:
          IL.Emit(OpCodes.Stloc_0);
          return IL;

        case 1:
          IL.Emit(OpCodes.Stloc_1);
          return IL;

        case 2:
          IL.Emit(OpCodes.Stloc_2);
          return IL;

        case 3:
          IL.Emit(OpCodes.Stloc_3);
          return IL;
        default:
          IL.Emit(OpCodes.Stloc, value);
          return IL;
      }
    }

    /// <summary>基于Ldloc指令的压栈，自动选择最合适的指令</summary>
    /// <param name="IL">指令</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static ILGenerator Ldloc(this ILGenerator IL, Int32 value)
    {
      switch (value)
      {
        case 0:
          IL.Emit(OpCodes.Ldloc_0);
          return IL;

        case 1:
          IL.Emit(OpCodes.Ldloc_1);
          return IL;

        case 2:
          IL.Emit(OpCodes.Ldloc_2);
          return IL;

        case 3:
          IL.Emit(OpCodes.Ldloc_3);
          return IL;
        default:
          IL.Emit(OpCodes.Ldloc, value);
          return IL;
      }
    }

    /// <summary>查找对象中其引用当前位于计算堆栈的字段的值。</summary>
    /// <param name="IL">指令</param>
    /// <param name="field"></param>
    /// <returns></returns>
    public static ILGenerator Ldfld(this ILGenerator IL, FieldInfo field)
    {
      IL.Emit(OpCodes.Ldfld, field);
      return IL;
    }

    /// <summary>间接加载到计算堆栈</summary>
    /// <param name="IL">指令</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static ILGenerator Ldind(this ILGenerator IL, Type type)
    {
      if (!type.IsValueType)
        IL.Emit(OpCodes.Ldind_Ref);
      else if (type.IsEnum)
        IL.Ldind(Enum.GetUnderlyingType(type));
      else if (type == typeof(IntPtr))
        IL.Emit(OpCodes.Ldind_I4);
      else if (type == typeof(UIntPtr))
        IL.Emit(OpCodes.Ldind_I4);
      else
      {
        switch (Type.GetTypeCode(type))
        {
          case TypeCode.Boolean:
            IL.Emit(OpCodes.Ldind_I1);
            break;

          case TypeCode.SByte:
            IL.Emit(OpCodes.Ldind_I1);
            break;

          case TypeCode.Byte:
            IL.Emit(OpCodes.Ldind_U1);
            break;

          case TypeCode.Char:
            IL.Emit(OpCodes.Ldind_U2);
            break;

          case TypeCode.Int16:
            IL.Emit(OpCodes.Ldind_I2);
            break;

          case TypeCode.Int32:
            IL.Emit(OpCodes.Ldind_I4);
            break;

          case TypeCode.Int64:
            IL.Emit(OpCodes.Ldind_I8);
            break;

          case TypeCode.UInt16:
            IL.Emit(OpCodes.Ldind_U2);
            break;

          case TypeCode.UInt32:
            IL.Emit(OpCodes.Ldind_U4);
            break;

          case TypeCode.UInt64:
            IL.Emit(OpCodes.Ldind_I8);
            break;

          case TypeCode.Single:
            IL.Emit(OpCodes.Ldind_R4);
            break;

          case TypeCode.Double:
            IL.Emit(OpCodes.Ldind_R8);
            break;

          default:
            throw new Exception("{0}不支持的类型{1}".FormatWith("Ldind", type));
        }
      }
      return IL;
    }

    /// <summary>间接加载到计算堆栈</summary>
    /// <param name="IL">指令</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static ILGenerator Stind(this ILGenerator IL, Type type)
    {
      if (!type.IsValueType)
        IL.Emit(OpCodes.Stind_Ref);
      else if (type.IsEnum)
        IL.Stind(Enum.GetUnderlyingType(type));
      else if (type == typeof(IntPtr))
        IL.Emit(OpCodes.Stind_I4);
      else if (type == typeof(UIntPtr))
        IL.Emit(OpCodes.Stind_I4);
      else
      {
        switch (Type.GetTypeCode(type))
        {
          case TypeCode.Boolean:
            IL.Emit(OpCodes.Stind_I1);
            break;

          case TypeCode.SByte:
            IL.Emit(OpCodes.Stind_I1);
            break;

          case TypeCode.Byte:
            IL.Emit(OpCodes.Stind_I1);
            break;

          case TypeCode.Char:
            IL.Emit(OpCodes.Stind_I2);
            break;

          case TypeCode.Int16:
            IL.Emit(OpCodes.Stind_I2);
            break;

          case TypeCode.Int32:
            IL.Emit(OpCodes.Stind_I4);
            break;

          case TypeCode.Int64:
            IL.Emit(OpCodes.Stind_I8);
            break;

          case TypeCode.UInt16:
            IL.Emit(OpCodes.Stind_I2);
            break;

          case TypeCode.UInt32:
            IL.Emit(OpCodes.Stind_I4);
            break;

          case TypeCode.UInt64:
            IL.Emit(OpCodes.Stind_I8);
            break;

          case TypeCode.Single:
            IL.Emit(OpCodes.Stind_R4);
            break;

          case TypeCode.Double:
            IL.Emit(OpCodes.Stind_R8);
            break;

          default:
            throw new Exception("{0}不支持的类型{1}".FormatWith("Stind", type));
        }
      }
      return IL;
    }

    /// <summary>将位于指定数组索引处的包含对象引用的元素作为 O 类型（对象引用）加载到计算堆栈的顶部。</summary>
    /// <param name="IL">指令</param>
    /// <returns></returns>
    public static ILGenerator Ldelem_Ref(this ILGenerator IL)
    {
      IL.Emit(OpCodes.Ldelem_Ref);
      return IL;
    }

    /// <summary>用计算堆栈上的对象 ref 值（O 类型）替换给定索引处的数组元素。</summary>
    /// <param name="IL">指令</param>
    /// <returns></returns>
    public static ILGenerator Stelem_Ref(this ILGenerator IL)
    {
      IL.Emit(OpCodes.Stelem_Ref);
      return IL;
    }

    /// <summary>把一个类型转为指定类型，值类型装箱，引用类型直接Cast</summary>
    /// <param name="IL">指令</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static ILGenerator CastFromObject(this ILGenerator IL, Type type)
    {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (type != typeof(Object))
      {
        if (type.IsValueType)
          IL.Emit(OpCodes.Unbox_Any, type);
        else
          IL.Emit(OpCodes.Castclass, type);
      }
      return IL;
    }

    /// <summary>装箱</summary>
    /// <param name="IL">指令</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static ILGenerator BoxIfValueType(this ILGenerator IL, Type type)
    {
      if (type == null) throw new ArgumentNullException("type");
      if (type.IsValueType && type != typeof(void)) IL.Emit(OpCodes.Box, type);
      return IL;
    }

    /// <summary>调用</summary>
    /// <param name="IL">指令</param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static ILGenerator Call(this ILGenerator IL, MethodInfo method)
    {
      if (method.IsStatic || method.DeclaringType.IsValueType || !method.IsVirtual)
        IL.EmitCall(OpCodes.Call, method, null);
      else
        IL.EmitCall(OpCodes.Callvirt, method, null);

      //if (method.IsVirtual)
      //    IL.EmitCall(OpCodes.Callvirt, method, null);
      //else
      //    IL.EmitCall(OpCodes.Call, method, null);
      return IL;
    }

    /// <summary>加载空</summary>
    /// <param name="IL">指令</param>
    /// <returns></returns>
    public static ILGenerator Ldnull(this ILGenerator IL)
    {
      IL.Emit(OpCodes.Ldnull);
      return IL;
    }

    /// <summary>返回</summary>
    /// <param name="IL">指令</param>
    /// <returns></returns>
    public static ILGenerator Ret(this ILGenerator IL)
    {
      IL.Emit(OpCodes.Ret);
      return IL;
    }

    #endregion

    #region -- 创建对象 --

    /// <summary>创建值类型，对象位于栈上</summary>
    /// <param name="IL">指令</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static ILGenerator NewValueType(this ILGenerator IL, Type type)
    {
      // 声明目标类型的本地变量
      //Int32 index = IL.DeclareLocal(type).LocalIndex;
      IL.DeclareLocal(type);

      // 加载地址
      IL.Emit(OpCodes.Ldloca_S, 0);

      // 创建对象
      IL.Emit(OpCodes.Initobj, type);

      // 加载对象
      IL.Emit(OpCodes.Ldloc_0);
      return IL;
    }

    /// <summary>创建数组，参数必须是Object[]</summary>
    /// <param name="IL">指令</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static ILGenerator NewArray(this ILGenerator IL, Type type)
    {
      IL.Emit(OpCodes.Newarr, type);
      return IL;
    }

    /// <summary>创建对象</summary>
    /// <param name="IL">指令</param>
    /// <param name="constructor"></param>
    /// <returns></returns>
    public static ILGenerator NewObj(this ILGenerator IL, ConstructorInfo constructor)
    {
      IL.Emit(OpCodes.Newobj, constructor);
      return IL;
    }

    #endregion

    #region -- 复杂方法 --

    /// <summary>为引用参数声明本地变量</summary>
    /// <param name="IL">指令</param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static ILGenerator CreateLocalsForByRefParams(this ILGenerator IL, MethodBase method)
    {
      Int32 firstParamIndex = method.IsStatic ? 0 : 1;
      Int32 refParams = 0;
      ParameterInfo[] ps = method.GetParameters();

      for (Int32 i = 0; i < ps.Length; i++)
      {
        // 处理引用类型参数
        if (!ps[i].ParameterType.IsByRef) { continue; }
        Type type = ps[i].ParameterType.GetElementType();
        IL.DeclareLocal(type);

        // 处理输出类型
        if (ps[i].IsOut)
        {
          IL.Ldarg(firstParamIndex)
              .Ldc_I4(i)
              .Ldelem_Ref()
              .CastFromObject(type)
              .Stloc(refParams);
        }
        refParams++;
      }
      return IL;
    }

    /// <summary>将引用参数赋值到数组</summary>
    /// <param name="IL">指令</param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static ILGenerator AssignByRefParamsToArray(this ILGenerator IL, MethodBase method)
    {
      Int32 firstParamIndex = method.IsStatic ? 0 : 1;
      Int32 refParam = 0;
      ParameterInfo[] ps = method.GetParameters();

      for (Int32 i = 0; i < ps.Length; i++)
      {
        // 处理引用类型参数
        if (!ps[i].ParameterType.IsByRef) { continue; }
        IL.Ldarg(firstParamIndex)
            .Ldc_I4(i)
            .Ldloc(refParam++)
            .BoxIfValueType(ps[i].ParameterType.GetElementType())
            .Stelem_Ref();
      }
      return IL;
    }

    /// <summary>将参数压栈</summary>
    /// <param name="IL">指令</param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static ILGenerator PushParamsOrLocalsToStack(this ILGenerator IL, MethodBase method)
    {
      Int32 firstParamIndex = method.IsStatic ? 0 : 1;
      Int32 refParam = 0;
      ParameterInfo[] ps = method.GetParameters();

      for (Int32 i = 0; i < ps.Length; i++)
      {
        if (ps[i].ParameterType.IsByRef)
        {
          IL.Emit(OpCodes.Ldloc_S, refParam++);
        }
        else
        {
          IL.Ldarg(firstParamIndex)
              .Ldc_I4(i)
              .Ldelem_Ref()
              .CastFromObject(ps[i].ParameterType);
        }
      }
      return IL;
    }

    /// <summary>将指定参数位置的数组参数按照方法参数要求压栈</summary>
    /// <param name="IL">指令</param>
    /// <param name="paramIndex"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static ILGenerator PushParams(this ILGenerator IL, Int32 paramIndex, MethodBase method)
    {
      ParameterInfo[] ps = method.GetParameters();

      for (Int32 i = 0; i < ps.Length; i++)
      {
        IL.Ldarg(paramIndex)
            .Ldc_I4(i)
            .Ldelem_Ref()
            .CastFromObject(ps[i].ParameterType);
      }
      return IL;
    }

    /// <summary>将指定参数位置的数组参数一个个压栈</summary>
    /// <param name="IL">指令</param>
    /// <param name="paramIndex"></param>
    /// <param name="paramTypes"></param>
    /// <returns></returns>
    public static ILGenerator PushParams(this ILGenerator IL, Int32 paramIndex, Type[] paramTypes)
    {
      if (paramTypes == null || paramTypes.Length < 1) return IL;

      for (Int32 i = 0; i < paramTypes.Length; i++)
      {
        IL.Ldarg(paramIndex)
            .Ldc_I4(i)
            .Ldelem_Ref()
            .CastFromObject(paramTypes[i]);
      }
      return IL;
    }

    #endregion

    #region -- Take from Fast.Reflection --

    public static ILGenerator ret(this ILGenerator il) { il.Emit(OpCodes.Ret); return il; }
    public static ILGenerator cast(this ILGenerator il, Type type) { il.Emit(OpCodes.Castclass, type); return il; }
    public static ILGenerator box(this ILGenerator il, Type type) { il.Emit(OpCodes.Box, type); return il; }
    public static ILGenerator unbox_any(this ILGenerator il, Type type) { il.Emit(OpCodes.Unbox_Any, type); return il; }
    public static ILGenerator unbox(this ILGenerator il, Type type) { il.Emit(OpCodes.Unbox, type); return il; }
    public static ILGenerator call(this ILGenerator il, MethodInfo method) { il.Emit(OpCodes.Call, method); return il; }
    public static ILGenerator callvirt(this ILGenerator il, MethodInfo method) { il.Emit(OpCodes.Callvirt, method); return il; }
    public static ILGenerator ldnull(this ILGenerator il) { il.Emit(OpCodes.Ldnull); return il; }
    public static ILGenerator bne_un(this ILGenerator il, Label target) { il.Emit(OpCodes.Bne_Un, target); return il; }
    public static ILGenerator beq(this ILGenerator il, Label target) { il.Emit(OpCodes.Beq, target); return il; }
    public static ILGenerator ldc_i4_0(this ILGenerator il) { il.Emit(OpCodes.Ldc_I4_0); return il; }
    public static ILGenerator ldc_i4_1(this ILGenerator il) { il.Emit(OpCodes.Ldc_I4_1); return il; }
    public static ILGenerator ldc_i4(this ILGenerator il, int c) { il.Emit(OpCodes.Ldc_I4, c); return il; }
    public static ILGenerator ldc_r4(this ILGenerator il, float c) { il.Emit(OpCodes.Ldc_R4, c); return il; }
    public static ILGenerator ldc_r8(this ILGenerator il, double c) { il.Emit(OpCodes.Ldc_R8, c); return il; }
    public static ILGenerator ldarg0(this ILGenerator il) { il.Emit(OpCodes.Ldarg_0); return il; }
    public static ILGenerator ldarg1(this ILGenerator il) { il.Emit(OpCodes.Ldarg_1); return il; }
    public static ILGenerator ldarg2(this ILGenerator il) { il.Emit(OpCodes.Ldarg_2); return il; }
    public static ILGenerator ldarga(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldarga, idx); return il; }
    public static ILGenerator ldarga_s(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldarga_S, idx); return il; }
    public static ILGenerator ldarg(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldarg, idx); return il; }
    public static ILGenerator ldarg_s(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldarg_S, idx); return il; }
    public static ILGenerator ldstr(this ILGenerator il, string str) { il.Emit(OpCodes.Ldstr, str); return il; }
    public static ILGenerator ifclass_ldind_ref(this ILGenerator il, Type type) { if (!type.IsValueType) il.Emit(OpCodes.Ldind_Ref); return il; }
    public static ILGenerator ldloc0(this ILGenerator il) { il.Emit(OpCodes.Ldloc_0); return il; }
    public static ILGenerator ldloc1(this ILGenerator il) { il.Emit(OpCodes.Ldloc_1); return il; }
    public static ILGenerator ldloc2(this ILGenerator il) { il.Emit(OpCodes.Ldloc_2); return il; }
    public static ILGenerator ldloca_s(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldloca_S, idx); return il; }
    public static ILGenerator ldloca_s(this ILGenerator il, LocalBuilder local) { il.Emit(OpCodes.Ldloca_S, local); return il; }
    public static ILGenerator ldloc_s(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldloc_S, idx); return il; }
    public static ILGenerator ldloc_s(this ILGenerator il, LocalBuilder local) { il.Emit(OpCodes.Ldloc_S, local); return il; }
    public static ILGenerator ldloca(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldloca, idx); return il; }
    public static ILGenerator ldloca(this ILGenerator il, LocalBuilder local) { il.Emit(OpCodes.Ldloca, local); return il; }
    public static ILGenerator ldloc(this ILGenerator il, int idx) { il.Emit(OpCodes.Ldloc, idx); return il; }
    public static ILGenerator ldloc(this ILGenerator il, LocalBuilder local) { il.Emit(OpCodes.Ldloc, local); return il; }
    public static ILGenerator initobj(this ILGenerator il, Type type) { il.Emit(OpCodes.Initobj, type); return il; }
    public static ILGenerator newobj(this ILGenerator il, ConstructorInfo ctor) { il.Emit(OpCodes.Newobj, ctor); return il; }
    public static ILGenerator Throw(this ILGenerator il) { il.Emit(OpCodes.Throw); return il; }
    public static ILGenerator throw_new(this ILGenerator il, Type type) { var exp = type.GetConstructor(Type.EmptyTypes); newobj(il, exp).Throw(); return il; }
    public static ILGenerator stelem_ref(this ILGenerator il) { il.Emit(OpCodes.Stelem_Ref); return il; }
    public static ILGenerator ldelem_ref(this ILGenerator il) { il.Emit(OpCodes.Ldelem_Ref); return il; }
    public static ILGenerator ldlen(this ILGenerator il) { il.Emit(OpCodes.Ldlen); return il; }
    public static ILGenerator stloc(this ILGenerator il, int idx) { il.Emit(OpCodes.Stloc, idx); return il; }
    public static ILGenerator stloc_s(this ILGenerator il, int idx) { il.Emit(OpCodes.Stloc_S, idx); return il; }
    public static ILGenerator stloc(this ILGenerator il, LocalBuilder local) { il.Emit(OpCodes.Stloc, local); return il; }
    public static ILGenerator stloc_s(this ILGenerator il, LocalBuilder local) { il.Emit(OpCodes.Stloc_S, local); return il; }
    public static ILGenerator stloc0(this ILGenerator il) { il.Emit(OpCodes.Stloc_0); return il; }
    public static ILGenerator stloc1(this ILGenerator il) { il.Emit(OpCodes.Stloc_1); return il; }
    public static ILGenerator mark(this ILGenerator il, Label label) { il.MarkLabel(label); return il; }
    public static ILGenerator ldfld(this ILGenerator il, FieldInfo field) { il.Emit(OpCodes.Ldfld, field); return il; }
    public static ILGenerator ldsfld(this ILGenerator il, FieldInfo field) { il.Emit(OpCodes.Ldsfld, field); return il; }
    public static ILGenerator lodfld(this ILGenerator il, FieldInfo field) { if (field.IsStatic) ldsfld(il, field); else ldfld(il, field); return il; }
    public static ILGenerator ifvaluetype_box(this ILGenerator il, Type type) { if (type.IsValueType) il.Emit(OpCodes.Box, type); return il; }
    public static ILGenerator stfld(this ILGenerator il, FieldInfo field) { il.Emit(OpCodes.Stfld, field); return il; }
    public static ILGenerator stsfld(this ILGenerator il, FieldInfo field) { il.Emit(OpCodes.Stsfld, field); return il; }
    public static ILGenerator setfld(this ILGenerator il, FieldInfo field) { if (field.IsStatic) stsfld(il, field); else stfld(il, field); return il; }
    public static ILGenerator unboxorcast(this ILGenerator il, Type type) { if (type.IsValueType) unbox(il, type); else cast(il, type); return il; }
    public static ILGenerator callorvirt(this ILGenerator il, MethodInfo method) { if (method.IsVirtual) il.Emit(OpCodes.Callvirt, method); else il.Emit(OpCodes.Call, method); return il; }
    public static ILGenerator stind_ref(this ILGenerator il) { il.Emit(OpCodes.Stind_Ref); return il; }
    public static ILGenerator ldind_ref(this ILGenerator il) { il.Emit(OpCodes.Ldind_Ref); return il; }
    public static LocalBuilder declocal(this ILGenerator il, Type type) { return il.DeclareLocal(type); }
    public static Label deflabel(this ILGenerator il) { return il.DefineLabel(); }
    public static ILGenerator ifclass_ldarg_else_ldarga(this ILGenerator il, int idx, Type type) { if (type.IsValueType) il.ldarga(idx); else il.ldarg(idx); return il; }
    public static ILGenerator ifclass_ldloc_else_ldloca(this ILGenerator il, int idx, Type type) { if (type.IsValueType) il.ldloca(idx); else il.ldloc(idx); return il; }
    public static ILGenerator perform(this ILGenerator il, Action<ILGenerator, MemberInfo> action, MemberInfo member) { action(il, member); return il; }
    public static ILGenerator ifbyref_ldloca_else_ldloc(this ILGenerator il, LocalBuilder local, Type type) { if (type.IsByRef) ldloca(il, local); else ldloc(il, local); return il; }

    #endregion
  }
}