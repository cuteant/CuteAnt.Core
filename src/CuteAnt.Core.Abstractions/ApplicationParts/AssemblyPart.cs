using System;
using System.Diagnostics;
using System.Reflection;

namespace CuteAnt.ApplicationParts
{
  /// <summary>An <see cref="IApplicationPart"/> backed by an <see cref="Assembly"/>.</summary>
  [DebuggerDisplay("{" + nameof(Assembly) + "}")]
  public class AssemblyPart : ApplicationPart, IApplicationPart
  {
    /// <summary>Initializes a new <see cref="AssemblyPart"/> instance.</summary>
    /// <param name="assembly"></param>
    public AssemblyPart(Assembly assembly)
    {
      if (null == assembly) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.assembly); }
      this.Assembly = assembly;
    }

    /// <summary>Gets the <see cref="Assembly"/> of the <see cref="IApplicationPart"/>.</summary>
    public Assembly Assembly { get; }

    /// <summary>Gets the name of the <see cref="ApplicationPart"/>.</summary>
    public override string Name => this.Assembly.GetName().Name;

    /// <summary>Returns <see langword="true"/> if this instance is equivalent to the provided instance, <see langword="false"/> otherwise.</summary>
    /// <param name="other">The other instance/</param>
    /// <returns><see langword="true"/> if this instance is equivalent to the provided instance, <see langword="false"/> otherwise.</returns>
    protected bool Equals(AssemblyPart other)
    {
      return Equals(this.Assembly, other?.Assembly);
    }

    /// <inheritdoc />
    public bool Equals(IApplicationPart other) => this.Equals(other as AssemblyPart);

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((AssemblyPart)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return this.Assembly != null ? this.Assembly.GetHashCode() : 0;
    }
  }
}
