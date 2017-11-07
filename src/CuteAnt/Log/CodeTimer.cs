#if DESKTOPCLR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CuteAnt.Properties;

namespace CuteAnt.Log
{
	#region -- class CodeTimerResult --

	/// <summary>表示 <see cref="CodeTimer"/> 执行结果的类.</summary>
	public class CodeTimerResult
	{
		/// <summary>初始化 <see cref="CodeTimer"/> 类的新实例.</summary>
		public CodeTimerResult()
		{
			GenerationList = new Int32[GC.MaxGeneration + 1];
		}

		/// <summary>名称.</summary>
		public String Name { get; set; }

		/// <summary>运行时间.</summary>
		public Int64 TimeElapsed { get; set; }

		/// <summary>Cpu 时钟周期.</summary>
		public UInt64 CpuCycles { get; set; }

		/// <summary>线程时间，单位是100ns，除以10000转为ms.</summary>
		public Int64 ThreadTime { get; set; }

		/// <summary>GC 代数集合.</summary>
		public Int32[] GenerationList { get; set; }

		/// <summary>线程的计数.</summary>
		public Int32 ThreadCount { get; set; }

		/// <summary>重复的次数.</summary>
		public Int32 Iteration { get; set; }

		/// <summary>模拟思考的时间.</summary>
		public Int32 MockThinkTime { get; set; }

		/// <summary>执行成功计数.</summary>
		public Int32 SuccessCount { get; set; }

		/// <summary>执行失败计数.</summary>
		public Int32 FailureCount { get; set; }

		/// <summary>重置 <see cref="CodeTimer"/>.</summary>
		/// <returns>重置后的 <see cref="CodeTimer"/> 对象实例.</returns>
		public CodeTimerResult Reset()
		{
			Name = String.Empty;
			TimeElapsed = 0;
			CpuCycles = 0;
			GenerationList = new Int32[GC.MaxGeneration + 1];
			return this;
		}
	}

	#endregion

	#region -- class CodeTimer --

	/// <summary>代码性能计时器.</summary>
	public static class CodeTimer
	{
		/// <summary>是否输出详细的数据.</summary>
		public static Boolean IsPrintDetail = false;

		/// <summary>输出格式.</summary>
		public static Boolean IsPrintExecuteResultLine = true;

		/*
		 * Initialize 方法应该在测试开始前调用.
		 *	首先它会把当前进程及当前线程的优先级设为最高,这样便可以相对减少操作系统在调度上造成的干扰.
		 *	然后调用一次 Time 方法进行“预热”,让 JIT 将 IL 编译成本地代码,让 Time 方法尽快“进入状态”.
		 *	Execute 方法则是真正用于性能计数的方法.
		 * CPU 时钟周期是性能计数中的辅助参考
		 * 	说明 CPU 分配了多少时间片给这段方法来执行,它和消耗时间并没有必然联系.
		 * 	例如 Thread.Sleep 方法会让CPU暂时停止对当前线程的“供给”,这样虽然消耗了时间,但是节省了CPU时钟周期 :
		 * 		CodeTimer.Time("Thread Sleep", 1, () =&gt; { Thread.Sleep(3000); });
		 * 		CodeTimer.Time("Empty Method", 10000000, () =&gt; { });
		 * 垃圾收集次数的统计,即直观地反应了方法资源分配(消耗)的规模 :
		 * 	Int32 iteration = 100 * 1000;
		 * 	String s = "";
		 * 	CodeTimer.Execute("String Concat", iteration, () =&gt; { s += "a"; });
		 * 	StringBuilder sb = new StringBuilder();
		 * 	CodeTimer.Execute("StringBuilder", iteration, () =&gt; { sb.Append("a"); });
		 */

		#region - 单线程 -

		/// <summary>单线程 Execute 方法的初始化.</summary>
		public static void InitializeBySingle()
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			Execute("", 1, () => { }, null);
		}

		/// <summary>使用单线程的方式执行 <paramref name="action"/>,并打印出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="action">需要执行的方法体.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static CodeTimerResult Execute(String name, Int32 iteration, Action action)
		{
			return Execute(name, iteration, () => { action(); return true; }, Console.WriteLine);
		}

		/// <summary>使用单线程的方式执行 <paramref name="func"/>,并打印出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="func">需要执行的方法体.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static CodeTimerResult Execute(String name, Int32 iteration, Func<Boolean> func)
		{
			return Execute(name, iteration, func, Console.WriteLine);
		}

		/// <summary>使用单线程的方式执行 <paramref name="action"/>,并使用 <paramref name="Output"/> 方法输出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="action">需要执行的方法体.</param>
		/// <param name="Output">要写入的输出流.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static CodeTimerResult Execute(String name, Int32 iteration, Action action, Action<String> Output)
		{
			return Execute(name, iteration, () => { action(); return true; }, Output);
		}

		/// <summary>使用单线程的方式执行 <paramref name="func"/>,并使用 <paramref name="Output"/> 方法输出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="func">需要执行的方法体.</param>
		/// <param name="Output">要写入的输出流.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static CodeTimerResult Execute(String name, Int32 iteration, Func<Boolean> func, Action<String> Output)
		{
			if (name == null) name = String.Empty;
			if (func == null) { return null; }

			CodeTimerResult result = new CodeTimerResult();
			Int32 successCount = 0;
			Int32 failureCount = 0;

			#region 强制 GC 进行收集,并记录目前各代已经收集的次数.

			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			Int32[] gcCounts = new Int32[GC.MaxGeneration + 1];

			for (Int32 i = 0; i <= GC.MaxGeneration; i++)
			{
				gcCounts[i] = GC.CollectionCount(i);
			}

			#endregion

			#region 执行代码,记录下消耗的时间及 CPU 时钟周期.

			Stopwatch watch = new Stopwatch();
			watch.Start();
			UInt64 cycleCount = GetCycleCount();
			Int64 threadTime = GetCurrentThreadTimes();

			for (Int32 i = 0; i < iteration; i++)
			{
				if (func()) ++successCount;
				else ++failureCount;
			}
			result.CpuCycles = GetCycleCount() - cycleCount;
			result.ThreadTime = GetCurrentThreadTimes() - threadTime;
			watch.Stop();

			#endregion

			#region 收集数据

			if (IsPrintExecuteResultLine)
			{
				result.Name = name;
			}
			else
			{
				result.Name = String.Format(Resources.SingleThreadName, name, Resources.NameSuffix);
			}
			result.TimeElapsed = watch.ElapsedMilliseconds;

			for (Int32 i = 0; i <= GC.MaxGeneration; i++)
			{
				Int32 count = GC.CollectionCount(i) - gcCounts[i];
				result.GenerationList[i] = count;
			}
			result.SuccessCount = successCount;
			result.FailureCount = failureCount;
			result.ThreadCount = 1;
			result.Iteration = iteration;
			if (IsPrintExecuteResultLine)
			{
				PrintExecuteResultLine(result, Output);
			}
			else
			{
				PrintExecuteResult(result, Output);
			}

			#endregion

			return result;
		}

		#endregion

		#region - 多线程 -

		/// <summary>多线程并发 Execute 方法的初始化.</summary>
		public static void InitializeByConcurrent()
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			Execute("", 1, 1, 0, () => { return true; }, null);
		}

		/// <summary>使用多线程并发的方式执行 <paramref name="action"/>,并打印出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="threadCount">线程的计数.</param>
		/// <param name="action">需要执行的方法体.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static List<CodeTimerResult> Execute(String name, Int32 iteration, Int32 threadCount, Action action)
		{
			return Execute(name, iteration, threadCount, 0, () => { action(); return true; }, Console.WriteLine);
		}

		/// <summary>使用多线程并发的方式执行 <paramref name="func"/>,并打印出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="threadCount">线程的计数.</param>
		/// <param name="func">需要执行的方法体.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static List<CodeTimerResult> Execute(String name, Int32 iteration, Int32 threadCount, Func<Boolean> func)
		{
			return Execute(name, iteration, threadCount, 0, func, Console.WriteLine);
		}

		/// <summary>使用多线程并发的方式执行 <paramref name="action"/>,并使用 <paramref name="Output"/> 方法输出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="threadCount">线程的计数.</param>
		/// <param name="action">需要执行的方法体.</param>
		/// <param name="Output">要写入的输出流.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static List<CodeTimerResult> Execute(String name, Int32 iteration, Int32 threadCount, Action action, Action<String> Output)
		{
			return Execute(name, iteration, threadCount, 0, () => { action(); return true; }, Output);
		}

		/// <summary>使用多线程并发的方式执行 <paramref name="func"/>,并使用 <paramref name="Output"/> 方法输出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="threadCount">线程的计数.</param>
		/// <param name="func">需要执行的方法体.</param>
		/// <param name="Output">要写入的输出流.</param>
		public static List<CodeTimerResult> Execute(String name, Int32 iteration, Int32 threadCount, Func<Boolean> func, Action<String> Output)
		{
			return Execute(name, iteration, threadCount, 0, func, Output);
		}

		/// <summary>使用多线程并发的方式执行 <paramref name="action"/>,并使用 <paramref name="Output"/> 方法输出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="threadCount">线程的计数.</param>
		/// <param name="mockThinkTime">模拟思考的时间.</param>
		/// <param name="action">需要执行的方法体.</param>
		/// <param name="Output">要写入的输出流.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static List<CodeTimerResult> Execute(String name, Int32 iteration, Int32 threadCount, Int32 mockThinkTime, Action action, Action<String> Output)
		{
			return Execute(name, iteration, threadCount, mockThinkTime, () => { action(); return true; }, Output);
		}

		/// <summary>使用多线程并发的方式执行 <paramref name="func"/>,并使用 <paramref name="Output"/> 方法输出 : 方法的运行时间, Cpu 时钟周期, 各代垃圾收集的回收次数的统计.</summary>
		/// <param name="name">名称.</param>
		/// <param name="iteration">循环次数.</param>
		/// <param name="threadCount">线程的计数.</param>
		/// <param name="mockThinkTime">模拟思考的时间.</param>
		/// <param name="func">需要执行的方法体.</param>
		/// <param name="Output">要写入的输出流.</param>
		/// <remarks>一个 <see cref="CodeTimerResult"/> 表示执行的结果.</remarks>
		public static List<CodeTimerResult> Execute(String name, Int32 iteration, Int32 threadCount, Int32 mockThinkTime, Func<Boolean> func, Action<String> Output)
		{
			if (name == null) name = String.Empty;
			if (threadCount < 0) throw new ArgumentOutOfRangeException("threadCount", Resources.MessageThan0);
			if (iteration < 0) throw new ArgumentOutOfRangeException("threadCount", Resources.MessageThan0);
			if (mockThinkTime < 0) throw new ArgumentOutOfRangeException("thinkTime", Resources.MessageThan0);
			if (func == null) return null;
			IsPrintDetail = true;
			List<CodeTimerResult> results = new List<CodeTimerResult>(threadCount);
			CodeTimerResult totalResult = new CodeTimerResult();
			ManualResetEvent manualResetEvent = new ManualResetEvent(true);		// 主线程控制信号
			ManualResetEvent threadResetEvent = new ManualResetEvent(true); 	// 子线程线程控制信号
			Int32 currentThreadIndex;												// 当前线程索引值

			for (Int32 repeat = 0; repeat < iteration; repeat++)
			{
				manualResetEvent.Reset();										// 主线程进入阻止状态
				threadResetEvent.Reset();										// 子线程进入阻止状态
				currentThreadIndex = 0;

				for (Int32 i = 0; i < threadCount; i++)
				{
					Thread thread = new Thread(
						(threadIndex) =>
						{
							CodeTimerResult executeResult = new CodeTimerResult();
							Interlocked.Increment(ref currentThreadIndex);
							if (currentThreadIndex < threadCount)
							{
								threadResetEvent.WaitOne();			// 等待所有线程创建完毕后同时执行测试
							}
							else
							{
								threadResetEvent.Set();				// 最后一个线程创建完成,通知所有线程,开始执行测试
							}

							// 执行测试,委托给 SingleThreadExecute 方法来做.
							executeResult = Execute(
								String.Format(Resources.ChildThreadName, name, threadIndex, Resources.NameSuffix),
								1,
								func,
								null
							);
							Interlocked.Decrement(ref currentThreadIndex);
							if (currentThreadIndex == 0)
							{
								results.Add(executeResult);			// 保存执行结果
								manualResetEvent.Set();				//通知主线程继续
							}
							else
							{
								results.Add(executeResult);			// 保存执行结果
							}
						}
					);
					thread.Start(i);
				}

				// 阻止主线程,等待子线程完成所有任务.
				manualResetEvent.WaitOne();
				Thread.Sleep(mockThinkTime);
			}
			totalResult.Name = String.Format(Resources.MultiThreadName, name, Resources.NameSuffix);
			results.ForEach(
				(item) =>
				{
					totalResult.TimeElapsed += item.TimeElapsed;
					totalResult.CpuCycles += item.CpuCycles;

					for (Int32 i = 0; i < totalResult.GenerationList.Length; i++)
					{
						totalResult.GenerationList[i] += item.GenerationList[i];
					}
					totalResult.SuccessCount += item.SuccessCount;
					totalResult.FailureCount += item.FailureCount;
				}
			);
			totalResult.ThreadCount = threadCount;
			totalResult.Iteration = iteration;
			PrintExecuteResult(totalResult, Output);

			// 释放资源
			manualResetEvent.Close();
			threadResetEvent.Close();
			return results;
		}

		#endregion

		#region - 打印 -

		/// <summary>显示头部</summary>
		/// <param name="title"></param>
		public static void ShowHeader(String title = "指标")
		{
			Write(title, 16);
			Console.Write("：");
			ConsoleColor currentForeColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Write("执行时间", 9);
			Console.Write(" ");
			Write("CPU时间", 9);
			Console.Write(" ");
			Write("指令周期", 15);
			Console.WriteLine("   GC(0/1/2)");
			Console.ForegroundColor = currentForeColor;
		}

		private static void Write(String name, Int32 max)
		{
			Int32 len = Encoding.Default.GetByteCount(name);
			if (len < max)
			{
				Console.Write(new String(' ', max - len));
			}
			Console.Write(name);
		}

		/// <summary>打印 <paramref name="result"/> 到 <paramref name="Output"/>.</summary>
		/// <param name="result"><see cref="CodeTimer"/> 的执行结果.</param>
		/// <param name="Output">要写入的输出流</param>
		public static void PrintExecuteResult(CodeTimerResult result, Action<String> Output)
		{
			if (Output != null)
			{
				ConsoleColor currentForeColor = Console.ForegroundColor;	// 保留当前控制台前景色,并使用黄色输出名称参数.
				Console.ForegroundColor = ConsoleColor.Yellow;
				if (result.Name.IsNullOrWhiteSpace()) { result.Name = Resources.NameSuffix; }
				Output(result.Name);
				Console.ForegroundColor = currentForeColor;					// 恢复控制台默认前景色,并打印出消耗时间及CPU时钟周期.
				Output(String.Format(Resources.TimeElapsed, result.TimeElapsed.ToString("N0")));
				Output(String.Format(Resources.CPUCycles, result.CpuCycles.ToString("N0")));

				// 打印执行过程中各代垃圾收集回收次数.
				for (Int32 i = 0; i < result.GenerationList.Length; i++)
				{
					Output(String.Format(Resources.CPUGen, i, result.GenerationList[i]));
				}
				if (IsPrintDetail)
				{
					Output(String.Format(Resources.ExecuteSuccessCount, result.SuccessCount.ToString()));
					Output(String.Format(Resources.ExecuteSuccessCount, result.FailureCount.ToString()));
				}

				// 循环次数 线程的计数 模拟思考的时间
				String stats = String.Format(Resources.IterationCount, result.Iteration);
				if (result.ThreadCount > 1) { stats = stats + String.Format(Resources.ThreadCount, result.ThreadCount); }
				if (result.MockThinkTime > 0) { stats = stats + String.Format(Resources.MockThinkTime, result.MockThinkTime); }
				Output(stats);
			}
		}

		public static void PrintExecuteResultLine(CodeTimerResult result, Action<String> Output)
		{
			if (Output != null)
			{
				if (result.Name.IsNullOrWhiteSpace()) { result.Name = Resources.NameSuffix; }
				var n = Encoding.Default.GetByteCount(result.Name);
				Console.Write("{0}{1}：", n >= 16 ? "" : new String(' ', 16 - n), result.Name);
				ConsoleColor currentForeColor = Console.ForegroundColor;	// 保留当前控制台前景色,并使用黄色输出名称参数.
				Console.ForegroundColor = ConsoleColor.Yellow;
				Output(String.Format("{0,7:n0}ms {1,7:n0}ms {2,15:n0} {3,3}/{4}/{5}", result.TimeElapsed, result.ThreadTime / 10000, result.CpuCycles, result.GenerationList[0], result.GenerationList[1], result.GenerationList[2]));
				Console.ForegroundColor = currentForeColor;
			}
		}

		#endregion

		#region - 辅助方法 -

		#region P/Invoke

		// 统计 CPU 时钟周期时.
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern Boolean GetThreadTimes(IntPtr hThread, out Int64 lpCreationTime, out Int64 lpExitTime, out Int64 lpKernelTime, out Int64 lpUserTime);

		// 统计 CPU 时钟周期时.(Vista 版本以上新的函数)
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern Boolean QueryThreadCycleTime(IntPtr threadHandle, ref UInt64 cycleTime);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetCurrentThread();

		private static Boolean supportCycle = true;

		#endregion

		private static UInt64 GetCycleCount()
		{
			//if (Environment.Version.Major < 6) return 0;
			if (!supportCycle) { return 0; }

			try
			{
				UInt64 cycleCount = 0;
				QueryThreadCycleTime(GetCurrentThread(), ref cycleCount);
				return cycleCount;
			}
			catch
			{
				supportCycle = false;
				return 0;
			}
		}

		private static Int64 GetCurrentThreadTimes()
		{
			Int64 l;
			Int64 kernelTime, userTimer;
			GetThreadTimes(GetCurrentThread(), out l, out l, out kernelTime, out userTimer);
			return kernelTime + userTimer;
		}

		#endregion
	}

	#endregion
}
#endif