using System.Reflection;

namespace CuteAnt.ApplicationParts
{
  /// <summary>A part of an application.</summary>
  public interface IApplicationPart
  {
    /// <summary>Gets the <see cref="Assembly"/> of the <see cref="IApplicationPart"/>.</summary>
    Assembly Assembly { get; }
  }
}
