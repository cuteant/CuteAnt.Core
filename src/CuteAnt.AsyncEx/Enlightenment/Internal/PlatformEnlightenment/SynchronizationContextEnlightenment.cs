using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CuteAnt.AsyncEx.Internal.PlatformEnlightenment
{
	internal static class SynchronizationContextEnlightenment
	{
		internal static void SetCurrentSynchronizationContext(SynchronizationContext context)
		{
			SynchronizationContext.SetSynchronizationContext(context);
		}
	}
}
