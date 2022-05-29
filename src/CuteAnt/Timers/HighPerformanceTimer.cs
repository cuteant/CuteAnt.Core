//#if NETFRAMEWORK
//using System;
//using System.Runtime.InteropServices;

//namespace CuteAnt.Threading
//{
//	/// <summary>基于系统性能计数器的定时器，计数单位是1微秒=1/1000毫秒。
//	/// 注意：该定时器会独占一个CPU核心，尝试定时器与主程序运行在同一核心将导致程序失去响应。
//	/// </summary>
//	/// <remarks>HighPerformanceTimer类，基于百度问答中的代码修改而成。</remarks>
//	public sealed class HighPerformanceTimer : IDisposable
//	{
//		#region -- Win32 API --

//		/// <summary>获取当前系统性能计数</summary>
//		/// <param name="lpPerformanceCount"></param>
//		/// <returns></returns>
//		[DllImport("Kernel32.dll")]
//		private static extern Boolean QueryPerformanceCounter(out Int64 lpPerformanceCount);

//		/// <summary>获取当前系统性能频率</summary>
//		/// <param name="lpFrequency"></param>
//		/// <returns></returns>
//		[DllImport("Kernel32.dll")]
//		private static extern Boolean QueryPerformanceFrequency(out Int64 lpFrequency);

//		/// <summary>指定某一特定线程运行在指定的CPU核心</summary>
//		/// <param name="hThread"></param>
//		/// <param name="dwThreadAffinityMask"></param>
//		/// <returns></returns>
//		[DllImport("kernel32.dll")]
//		private static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);

//		/// <summary>获取当前线程的Handler</summary>
//		/// <returns></returns>
//		[DllImport("kernel32.dll")]
//		private static extern IntPtr GetCurrentThread();

//		#endregion

//		#region -- 属性 --

//		/// <summary>是否销毁定时器</summary>
//		private Boolean _Dispose = false;

//		/// <summary>是否正在运行定时器</summary>
//		private Boolean _IsRuning = false;

//		/// <summary>首次启动延时（微秒）</summary>
//		private UInt32 _Delay = 0;

//		/// <summary>定时器周期（微秒）</summary>
//		private Int64 _Period = 10L;

//		/// <summary>定时器运行时独占的CPU核心索引序号</summary>
//		private Int32 _CpuIndex = 0;

//		/// <summary>系统性能计数频率（每秒）</summary>
//		private Int64 _Freq = 0L;

//		/// <summary>系统性能计数频率（每微秒）</summary>
//		private Int64 _Freqmms = 0L;

//		/// <summary>回调函数定义</summary>
//		private TickEventHandler Tick;

//		#endregion

//		#region -- 构造 --

//		/// <summary>定时器事件的委托定义</summary>
//		/// <param name="sender">事件的发起者，即定时器对象</param>
//		/// <param name="jumpPeriod">上次调用和本次调用跳跃的周期数</param>
//		/// <param name="interval">上次调用和本次调用之间的间隔时间（微秒）</param>
//		public delegate void TickEventHandler(HighPerformanceTimer sender, Int64 jumpPeriod, Int64 interval);

//		/// <summary>实例化一个高性能定时器</summary>
//		/// <param name="delay">首次启动定时器延时时间（微秒）</param>
//		/// <param name="period">定时器触发的周期（微秒）</param>
//		/// <param name="cpuIndex">指定定时器线程独占的CPU核心索引，默认为 0，自动分配最后一个 CPU核心，始终不允许为定时器分配0#CPU</param>
//		/// <param name="tick">定时器触发时的回调函数</param>
//		public HighPerformanceTimer(TickEventHandler tick, UInt32 delay, UInt32 period, Int32 cpuIndex = 0)
//		{
//			if (PlatformHelper.IsSingleProcessor) { throw new Exception("定时器无法运行在单 CPU 的机器上！"); }

//			Tick = tick;
//			_Delay = delay;
//			_Period = period;
//			if (cpuIndex == 0)
//			{
//				_CpuIndex = PlatformHelper.ProcessorCount - 1;
//			}
//			else
//			{
//				_CpuIndex = cpuIndex;
//			}
//			Int64 freq = 0L;
//			if (QueryPerformanceFrequency(out freq))
//			{
//				if (freq > 0L)
//				{
//					_Freq = freq;
//					_Freqmms = freq / 1000000L;//每微秒性能计数器跳跃次数
//				}
//				else
//				{
//					throw new Exception("初始化定时器失败");
//				}
//				if (_CpuIndex == 0)
//				{
//					throw new Exception("定时器不允许被分配到0#CPU");
//				}
//				if (_CpuIndex >= PlatformHelper.ProcessorCount)
//				{
//					throw new Exception("为定时器分配了超出索引的CPU");
//				}
//			}
//			else
//			{
//				throw new Exception("不支持高性能计数器！");
//			}
//		}

//		/// <summary>销毁当前定时器所占用的资源</summary>
//		public void Dispose()
//		{
//			_Dispose = true;
//			while (_IsRuning)
//			{
//				System.Windows.Forms.Application.DoEvents();//在工作未完成之前，允许处理消息队列，防止调用者挂起
//			}
//			if (_threadRumTimer != null)
//			{
//				_threadRumTimer.Abort();
//			}
//		}

//		#endregion

//		#region -- 计时器 --

//		private System.Threading.Thread _threadRumTimer;

//		/// <summary>开启定时器</summary>
//		public void Open()
//		{
//			if (Tick != null)
//			{
//				_threadRumTimer = new System.Threading.Thread(new System.Threading.ThreadStart(RunTimer));
//				_threadRumTimer.Start();
//			}
//		}

//		/// <summary>运行定时器</summary>
//		private void RunTimer()
//		{
//			var up = UIntPtr.Zero;
//			if (_CpuIndex != 0)
//			{
//				up = SetThreadAffinityMask(GetCurrentThread(), new UIntPtr(GetCpuID(_CpuIndex)));
//			}
//			if (up == UIntPtr.Zero) { throw new Exception("为定时器分配CPU核心时失败"); }

//			Int64 q1, q2;
//			QueryPerformanceCounter(out q1);
//			QueryPerformanceCounter(out q2);
//			if (_Delay > 0)
//			{
//				while (q2 < q1 + _Delay * _Freqmms)
//				{
//					QueryPerformanceCounter(out q2);
//				}
//			}
//			QueryPerformanceCounter(out q1);
//			QueryPerformanceCounter(out q2);
//			while (!_Dispose)
//			{
//				_IsRuning = true;
//				QueryPerformanceCounter(out q2);
//				if (q2 > q1 + _Freqmms * _Period)
//				{
//					//***********回调***********//
//					if (!_Dispose)
//					{
//						Tick(this, (q2 - q1) / (_Freqmms * _Period), (q2 - q1) / _Freqmms);
//					}
//					q1 = q2;
//					//System.Windows.Forms.Application.DoEvents();//会导致线程等待windows消息循环，时间损失15ms以上
//				}
//				_IsRuning = false;
//			}
//		}

//		/// <summary>根据CPU的索引序号获取CPU的标识序号</summary>
//		/// <param name="idx"></param>
//		/// <returns></returns>
//		private UInt64 GetCpuID(Int32 idx)
//		{
//			UInt64 cpuid = 0;
//			if (idx < 0 || idx >= PlatformHelper.ProcessorCount) { idx = 0; }

//			cpuid |= 1UL << idx;
//			return cpuid;
//		}

//		#endregion
//	}
//}
//#endif