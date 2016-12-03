using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if NET40
namespace CuteAnt.Extensions.Logging.NLog
#else
namespace Microsoft.Extensions.Logging.NLog
#endif
{
  public class NLogProviderOptions
  {
    /// <summary>Separator between for EventId.Id and EventId.Name. Default to .</summary>
    public string EventIdSeparator { get; set; }

    /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    public NLogProviderOptions()
    {
      EventIdSeparator = ".";
    }

    /// <summary>Default options</summary>
    internal static NLogProviderOptions Default = new NLogProviderOptions();
  }
}
