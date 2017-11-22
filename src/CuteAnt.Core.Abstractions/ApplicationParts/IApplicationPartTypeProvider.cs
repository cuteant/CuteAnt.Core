using System;
using System.Collections.Generic;
using System.Reflection;

namespace CuteAnt.ApplicationParts
{
  /// <summary>Exposes a set of types from an <see cref="ApplicationPart"/>.</summary>
  public interface IApplicationPartTypeProvider
  {
    /// <summary>Gets the list of available types in the <see cref="ApplicationPart"/>.</summary>
#if NET40
    IEnumerable<Type> Types { get; }
#else
    IEnumerable<TypeInfo> Types { get; }
#endif
  }
}
