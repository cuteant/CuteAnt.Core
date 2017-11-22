using System.Collections.Generic;

namespace CuteAnt.ApplicationParts
{
  /// <summary>Exposes one or more reference paths from an <see cref="ApplicationPart"/>.</summary>
  public interface ICompilationReferencesProvider
  {
    /// <summary>Gets reference paths used to perform runtime compilation.</summary>
    IEnumerable<string> GetReferencePaths();
  }
}
