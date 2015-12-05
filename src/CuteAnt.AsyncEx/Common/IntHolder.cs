using System;
using System.Threading;

namespace CuteAnt.AsyncEx
{
	public class IntHolder
	{
		private Int32 m_count;

		public Int32 Count
		{
			get { return m_count; }
		}

		public Int32 Increment()
		{
			return Interlocked.Increment(ref m_count);
		}
	}

	public class Int64Holder
	{
		private Int64 m_count;

		public Int64 Count
		{
			get { return m_count; }
		}

		public Int64 Increment()
		{
			return Interlocked.Increment(ref m_count);
		}
	}
}