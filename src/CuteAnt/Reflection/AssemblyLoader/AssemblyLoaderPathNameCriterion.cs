using System;
using System.Collections.Generic;

namespace CuteAnt.Reflection
{
  public class AssemblyLoaderPathNameCriterion : AssemblyLoaderCriterion
  {
    public new delegate bool Predicate(string pathName, out IEnumerable<string> complaints);

    public static AssemblyLoaderPathNameCriterion NewCriterion(Predicate predicate)
    {
      if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }

      return new AssemblyLoaderPathNameCriterion(predicate);
    }

    private AssemblyLoaderPathNameCriterion(Predicate predicate)
      : base((object input, out IEnumerable<string> complaints) => predicate((string)input, out complaints)) { }
  }
}
