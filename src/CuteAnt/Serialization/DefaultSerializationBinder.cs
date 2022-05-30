using System;
using System.Reflection;
using System.Runtime.Serialization;
using CuteAnt.Reflection;

namespace CuteAnt.Serialization
{
    /// <summary>The default serialization binder used when resolving and loading classes from type names.</summary>
    public class DefaultSerializationBinder : SerializationBinder
    {
        /// <summary>Initializes a new instance of the <see cref="DefaultSerializationBinder"/> class.</summary>
        public DefaultSerializationBinder()
        {
        }

        /// <summary>When overridden in a derived class, controls the binding of a serialized object to a type.</summary>
        /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="System.Type"/> name of the serialized object.</param>
        /// <returns>The type of the object the formatter creates a new instance of.</returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            return TypeUtils.ResolveType(new QualifiedType(assemblyName, typeName));
        }

        /// <summary>When overridden in a derived class, controls the binding of a serialized object to a type.</summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="System.Type"/> name of the serialized object.</param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            var typeDef = RuntimeTypeNameFormatter.GetTypeDefinition(serializedType);
            assemblyName = typeDef.AssemblyName;
            typeName = typeDef.TypeName;
        }
    }
}