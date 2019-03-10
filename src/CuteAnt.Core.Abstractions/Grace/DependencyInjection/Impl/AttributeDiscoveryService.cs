﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grace.Data.Immutable;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Service for discovering attributes and sharing them</summary>
  public interface IAttributeDiscoveryService
  {
    /// <summary>Get attributes from MethodInfo, PropertyInfo, ParameterInfo, and FieldInfo</summary>
    /// <param name="value">MethodInfo, PropertyInfo, ParameterInfo, and FieldInfo</param>
    /// <returns>attributes</returns>
    IEnumerable<Attribute> GetAttributes(object value);
  }

  /// <summary>attribute dicovery service</summary>
  public class AttributeDiscoveryService : IAttributeDiscoveryService
  {
    private ImmutableHashTree<object, IEnumerable<Attribute>> _knownValues = ImmutableHashTree<object, IEnumerable<Attribute>>.Empty;

    /// <summary>Get attributes from MethodInfo, PropertyInfo, ParameterInfo, and FieldInfo</summary>
    /// <param name="value">MethodInfo, PropertyInfo, ParameterInfo, and FieldInfo</param>
    /// <returns>attributes</returns>
    public IEnumerable<Attribute> GetAttributes(object value)
    {
      if (value == null) { return ImmutableLinkedList<Attribute>.Empty; }

      var values = _knownValues.GetValueOrDefault(value);

      if (values != null) { return values; }

      Attribute[] attributes;

      switch (value)
      {
        case Type type:
          attributes = type.GetCustomAttributes()?.ToArray();
          break;

        case ParameterInfo parameterInfo:
          attributes = parameterInfo.GetCustomAttributes()?.ToArray();
          break;

        case MemberInfo memberInfo:
          attributes = memberInfo.GetCustomAttributes<Attribute>()?.ToArray();
          break;

        default:
          throw new NotSupportedException($"Getting attributes on type {value.GetType().Name} is not supported");
      }

      if (attributes == null || attributes.Length == 0)
      {
        return ImmutableLinkedList<Attribute>.Empty;
      }

      var list = ImmutableLinkedList.From(attributes);

      return ImmutableHashTree.ThreadSafeAdd(ref _knownValues, value, list);
    }
  }
}