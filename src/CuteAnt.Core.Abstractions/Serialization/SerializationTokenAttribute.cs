using System;

namespace CuteAnt.Extensions.Serialization
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
  public class SerializationTokenAttribute : Attribute
  {
    public readonly SerializationToken Token;

    public SerializationTokenAttribute(SerializationToken token) => Token = token;
  }
}