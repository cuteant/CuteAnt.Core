using System;
using System.Timers;

namespace CuteAnt
{
	/// <summary>Simple timer implementation.</summary>
	public class TimerEx : Timer
	{
		/// <summary>Default contructor.</summary>
		public TimerEx()
			: base()
		{
		}

		/// <summary>Default contructor.</summary>
		/// <param name="interval">The time in milliseconds between events.</param>
		public TimerEx(Double interval)
			: base(interval)
		{
		}

		/// <summary>Default contructor.</summary>
		/// <param name="interval">The time in milliseconds between events.</param>
		/// <param name="autoReset">Specifies if timer is auto reseted.</param>
		public TimerEx(Double interval, Boolean autoReset)
			: base(interval)
		{
			this.AutoReset = autoReset;
		}

		// TODO: We need to do this class .NET CF compatible.
	}
}