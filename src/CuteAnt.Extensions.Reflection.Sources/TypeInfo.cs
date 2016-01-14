#if NET40
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Reflection
{
  /// <summary>Partial analog of TypeInfo existing in .NET 4.5 and higher.</summary>
  internal struct TypeInfo
  {
    #region @@ Fields @@

    private readonly Type _type;
    private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
    internal const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    #endregion

    #region @@ Constructors @@

    /// <summary>Creates type info by wrapping input type.</summary> <param name="type">Type to wrap.</param>
    public TypeInfo(Type type)
    {
      _type = type;
    }

    #endregion

#pragma warning disable 1591 // "Missing XML-comment"

    #region -- Properties --

    public Assembly Assembly { get { return _type.Assembly; } }
    public string AssemblyQualifiedName { get { return _type.AssemblyQualifiedName; } }
    public TypeAttributes Attributes { get { return _type.Attributes; } }
    public Type BaseType { get { return _type.BaseType; } }
    public bool ContainsGenericParameters { get { return _type.ContainsGenericParameters; } }
    public IEnumerable<CustomAttributeData> CustomAttributes { get { return _type.GetCustomAttributesData(); } }
    public IEnumerable<ConstructorInfo> DeclaredConstructors { get { return _type.GetConstructors(DeclaredOnlyLookup); } }
    public IEnumerable<EventInfo> DeclaredEvents { get { return _type.GetEvents(DeclaredOnlyLookup); } }
    public IEnumerable<FieldInfo> DeclaredFields { get { return _type.GetFields(DeclaredOnlyLookup); } }
    public IEnumerable<MemberInfo> DeclaredMembers { get { return _type.GetMembers(DeclaredOnlyLookup); } }
    public IEnumerable<MethodInfo> DeclaredMethods { get { return _type.GetMethods(DeclaredOnlyLookup); } }
    public IEnumerable<System.Reflection.TypeInfo> DeclaredNestedTypes
    {
      get
      {
        foreach (var t in _type.GetNestedTypes(DeclaredOnlyLookup))
        {
          yield return t.GetTypeInfo();
        }
      }
    }
    public IEnumerable<PropertyInfo> DeclaredProperties { get { return _type.GetProperties(DeclaredOnlyLookup); } }
    public MethodBase DeclaringMethod { get { return _type.DeclaringMethod; } }
    public Type DeclaringType { get { return _type.DeclaringType; } }
    public GenericParameterAttributes GenericParameterAttributes { get { return _type.GenericParameterAttributes; } }
    public int GenericParameterPosition { get { return _type.GenericParameterPosition; } }
    public Type[] GenericTypeParameters
    {
      get
      {
        if (_type.IsGenericTypeDefinition)
        {
          return _type.GetGenericArguments();
        }
        else
        {
          return Type.EmptyTypes;
        }
      }
    }

    public Type[] GenericTypeArguments
    {
      get
      {
        if (_type.IsGenericType && !_type.IsGenericTypeDefinition)
        {
          return _type.GetGenericArguments();
        }
        else
        {
          return Type.EmptyTypes;
        }
      }
    }
    public Guid GUID { get { return _type.GUID; } }
    public bool HasElementType { get { return _type.HasElementType; } }
    public IEnumerable<Type> ImplementedInterfaces { get { return _type.GetInterfaces(); } }
    public bool IsAbstract { get { return _type.IsAbstract; } }
    public bool IsAnsiClass { get { return _type.IsAnsiClass; } }
    public bool IsArray { get { return _type.IsArray; } }
    public bool IsAutoClass { get { return _type.IsAutoClass; } }
    public bool IsAutoLayout { get { return _type.IsAutoLayout; } }
    public bool IsByRef { get { return _type.IsByRef; } }
    public bool IsClass { get { return _type.IsClass; } }
    public bool IsCOMObject { get { return _type.IsCOMObject; } }
    public bool IsContextful { get { return _type.IsContextful; } }
    public bool IsEnum { get { return _type.IsEnum; } }
    public bool IsExplicitLayout { get { return _type.IsExplicitLayout; } }
    public bool IsGenericParameter { get { return _type.IsGenericParameter; } }
    public bool IsGenericType { get { return _type.IsGenericType; } }
    public bool IsGenericTypeDefinition { get { return _type.IsGenericTypeDefinition; } }
    public bool IsImport { get { return _type.IsImport; } }
    public bool IsInterface { get { return _type.IsInterface; } }
    public bool IsLayoutSequential { get { return _type.IsLayoutSequential; } }
    public bool IsMarshalByRef { get { return _type.IsMarshalByRef; } }
    public bool IsNested { get { return _type.IsNested; } }
    public bool IsNestedAssembly { get { return _type.IsNestedAssembly; } }
    public bool IsNestedFamANDAssem { get { return _type.IsNestedFamANDAssem; } }
    public bool IsNestedFamily { get { return _type.IsNestedFamily; } }
    public bool IsNestedFamORAssem { get { return _type.IsNestedFamORAssem; } }
    public bool IsNestedPrivate { get { return _type.IsNestedPrivate; } }
    public bool IsNestedPublic { get { return _type.IsNestedPublic; } }
    public bool IsNotPublic { get { return _type.IsNotPublic; } }
    public bool IsPointer { get { return _type.IsPointer; } }
    public bool IsPrimitive { get { return _type.IsPrimitive; } }
    public bool IsPublic { get { return _type.IsPublic; } }
    public bool IsSealed { get { return _type.IsSealed; } }
    public bool IsSecurityCritical { get { return _type.IsSecurityCritical; } }
    public bool IsSecuritySafeCritical { get { return _type.IsSecuritySafeCritical; } }
    public bool IsSerializable { get { return _type.IsSerializable; } }
    public bool IsSpecialName { get { return _type.IsSpecialName; } }
    public bool IsUnicodeClass { get { return _type.IsUnicodeClass; } }
    public bool IsValueType { get { return _type.IsValueType; } }
    public bool IsVisible { get { return _type.IsVisible; } }
    public MemberTypes MemberType { get { return _type.MemberType; } }
    public int MetadataToken { get { return _type.MetadataToken; } }
    public Module Module { get { return _type.Module; } }
    public string Name { get { return _type.Name; } }
    public string Namespace { get { return _type.Namespace; } }
    public Type ReflectedType { get { return _type.ReflectedType; } }
    public StructLayoutAttribute StructLayoutAttribute { get { return _type.StructLayoutAttribute; } }
    public RuntimeTypeHandle TypeHandle { get { return _type.TypeHandle; } }
    public ConstructorInfo TypeInitializer { get { return _type.TypeInitializer; } }
    public Type UnderlyingSystemType { get { return _type.UnderlyingSystemType; } }

    public bool IsConstructedGenericType { get { return _type.IsConstructedGenericType(); } }
    #endregion

    #region -- Methods --

    public Type AsType() { return _type; }

    //public IEnumerable<Attribute> GetCustomAttributes(Type attributeType, bool inherit)
    //{
    //  return _type.GetCustomAttributes(attributeType, inherit).Cast<Attribute>();
    //}

    public bool IsAssignableFrom(TypeInfo typeInfo) { return _type.IsAssignableFrom(typeInfo.AsType()); }
    public bool IsAssignableFrom(Type type) { return _type.IsAssignableFrom(type); }
    public bool IsSubclassOf(TypeInfo typeInfo) { return _type.IsSubclassOf(typeInfo.AsType()); }
    public bool IsSubclassOf(Type type) { return _type.IsSubclassOf(type); }

    public int GetArrayRank()
    {
      return _type.GetArrayRank();
    }

    public EventInfo GetDeclaredEvent(String name)
    {
      return _type.GetEvent(name, DeclaredOnlyLookup);
    }
    public FieldInfo GetDeclaredField(String name)
    {
      return _type.GetField(name, DeclaredOnlyLookup);
    }
    public MethodInfo GetDeclaredMethod(String name)
    {
      return _type.GetMethod(name, DeclaredOnlyLookup);
    }
    public IEnumerable<MethodInfo> GetDeclaredMethods(String name)
    {
      foreach (MethodInfo method in _type.GetMethods(DeclaredOnlyLookup))
      {
        if (method.Name == name)
          yield return method;
      }
    }
    public System.Reflection.TypeInfo GetDeclaredNestedType(String name)
    {
      var nt = _type.GetNestedType(name, DeclaredOnlyLookup);
      if (nt == null)
      {
        return default(TypeInfo); //the extension method GetTypeInfo throws for null
      }
      else
      {
        return nt.GetTypeInfo();
      }
    }
    public PropertyInfo GetDeclaredProperty(String name)
    {
      return _type.GetProperty(name, DeclaredOnlyLookup);
    }
    public Type[] GetGenericParameterConstraints() { return _type.GetGenericParameterConstraints(); }

    public Type GetElementType() { return _type.GetElementType(); }

    #endregion

#pragma warning restore 1591 // "Missing XML-comment"
  }
}
#endif