﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CuteAnt.Collections;
using CuteAnt.Reflection;
using CuteAnt.Configuration;
using ThreadState = System.Threading.ThreadState;
#if NET40
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace CuteAnt.Threading
{
  /// <summary>线程池。所有静态方法和实例方法均是线程安全。</summary>
  public sealed class ThreadPoolX : IDisposable
  {
    #region 基本属性
    private Int32 _MaxThreads;
    /// <summary>最大线程数</summary>
    public Int32 MaxThreads { get { return _MaxThreads; } set { _MaxThreads = value; } }

    private Int32 _MinThreads;
    /// <summary>最小线程数</summary>
    public Int32 MinThreads { get { return _MinThreads; } set { _MinThreads = value; } }

    private String _Name;
    /// <summary>线程池名称</summary>
    public String Name { get { return _Name; } set { _Name = value; } }

    private Int32 _Interval = 10;
    /// <summary>线程创建间隔。默认10毫秒</summary>
    public Int32 Interval { get { return _Interval; } set { _Interval = value; } }

    private Exception _LastError;
    /// <summary>最后的异常</summary>
    public Exception LastError { get { return _LastError; } set { _LastError = value; } }
    #endregion

    #region 线程
    /// <summary>用于维护管理线程的锁</summary>
    private readonly Object SynLock_mt = new Object();
    /// <summary>使用volatile关键字，等到对象创建完成</summary>
    private volatile Thread _ManagerThread;
    /// <summary>维护线程</summary>
    private Thread ManagerThread
    {
      get
      {
        if (_ManagerThread == null)
        {
          lock (SynLock_mt)
          {
            if (_ManagerThread == null)
            {
              var thread = new Thread(Work);
              //thread.Name = Name + "线程池维护线程";
              thread.Name = Name + "PM";
              thread.IsBackground = true;
              thread.Priority = ThreadPriority.Highest;//最高优先级
                                                       //留到第一个任务到来时再开始维护线程
                                                       //thread.Start();
              _ManagerThread = thread;
            }
          }
        }
        return _ManagerThread;
      }
    }

    /// <summary>第一个任务到来时初始化线程池</summary>
    private void Init()
    {
      var mt = ManagerThread;
      if (mt.IsAlive) return;
      if (!mt.ThreadState.HasFlag(ThreadState.Unstarted)) return;

      mt.Start();

      WriteLog("初始化线程池：" + Name + " 最大：" + MaxThreads + " 最小：" + MinThreads);
    }

    private List<ThreadX> _Threads = new List<ThreadX>();
    /// <summary>线程组。适用该资源时，记得加上线程锁lockObj</summary>
    private List<ThreadX> Threads { get { return _Threads; } }

    private Int32 _ThreadCount;
    /// <summary>当前线程数</summary>
    public Int32 ThreadCount { get { return _ThreadCount; } private set { _ThreadCount = value; } }

    private Int32 _RunningCount;
    /// <summary>正在处理任务的线程数</summary>
    public Int32 RunningCount { get { return _RunningCount; } private set { _RunningCount = value; } }

    private AutoResetEvent _Event = new AutoResetEvent(false);
    /// <summary>事件量</summary>
    private AutoResetEvent Event { get { return _Event; } }

    /// <summary>用户维护线程组的锁</summary>
    private Object SyncLock_Threads = new object();
    #endregion

    #region 任务队列
    private SortedList<Int32, ThreadTask> _Tasks = new SortedList<Int32, ThreadTask>();
    /// <summary>任务队列</summary>
    private SortedList<Int32, ThreadTask> Tasks { get { return _Tasks; } }

    /// <summary>任务队列同步锁</summary>
    private Object Sync_Tasks = new object();
    #endregion

    #region 构造
    /// <summary>构造一个线程池</summary>
    /// <param name="name">线程池名</param>
    private ThreadPoolX(String name)
    {
      Name = name;

      //最大线程数为4×处理器个数
      MaxThreads = 10 * Environment.ProcessorCount;
      MinThreads = 2 * Environment.ProcessorCount;
    }

    private static DictionaryCache<String, ThreadPoolX> _cache = new DictionaryCache<String, ThreadPoolX>();
    /// <summary>创建线程池。一个名字只能创建一个线程池。线程安全。</summary>
    /// <param name="name">线程池名</param>
    /// <returns></returns>
    public static ThreadPoolX Create(String name)
    {
      if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(name, "线程池名字不能为空！");

      return _cache.GetItem(name, key => new ThreadPoolX(key));
    }

    private static ThreadPoolX _Instance;
    /// <summary>默认线程池</summary>
    public static ThreadPoolX Instance
    {
      get
      {
        if (_Instance == null)
        {
          _Instance = Create("X");
        }
        return _Instance;
      }
      set { _Instance = value; }
    }
    #endregion

    #region 队列操作
    /// <summary>把用户工作项放入队列</summary>
    /// <param name="method">任务方法</param>
    /// <returns>任务编号</returns>
    public Int32 Queue(WaitCallback method) { return Queue(method, null); }

    /// <summary>把用户工作项放入队列</summary>
    /// <param name="method">任务方法</param>
    /// <param name="argument">任务参数</param>
    /// <returns>任务编号</returns>
    public Int32 Queue(WaitCallback method, Object argument) { return Queue(new ThreadTask(method, argument)); }

    /// <summary>把用户工作项放入队列。指定任务被取消时执行的方法，该方法仅针对尚未被线程开始调用时的任务有效</summary>
    /// <param name="method">任务方法</param>
    /// <param name="abortMethod">任务被取消时执行的方法</param>
    /// <param name="argument">任务参数</param>
    /// <returns>任务编号</returns>
    public Int32 Queue(WaitCallback method, WaitCallback abortMethod, Object argument)
    {
      return Queue(new ThreadTask(method, abortMethod, argument));
    }

    /// <summary>把用户工作项放入队列</summary>
    /// <param name="task">任务</param>
    /// <returns>任务编号</returns>
    private Int32 Queue(ThreadTask task)
    {
      //加锁，防止冲突
      lock (Sync_Tasks)
      {
        Tasks.Add(task.ID, task);

        //初始化线程池
        if (!ManagerThread.IsAlive) Init();
      }

      //通知管理线程，任务到达
      Event.Set();

      return task.ID;
    }

    /// <summary>取消任务</summary>
    /// <param name="id">任务编号</param>
    /// <returns>任务状态</returns>
    public TaskState Abort(Int32 id)
    {
      // 重点：
      // 这里使用了锁，很危险，所以仅仅在锁里面删除任务，任务的善后处理在锁外面完成

      // 要取消的任务
      ThreadTask task = null;
      // 任务状态
      var state = TaskState.Finished;

      #region 检查任务是否还在队列里面
      if (Tasks.ContainsKey(id))
      {
        //加锁，防止冲突
        lock (Sync_Tasks)
        {
          if (Tasks.ContainsKey(id))
          {
            task = Tasks[id];

            Tasks.Remove(id);
            state = TaskState.Unstarted;
          }
        }
      }
      #endregion

      #region 检查任务是否正在处理
      var ths = Threads;
      if (task == null && ths.Count > 0)
      {
        lock (SyncLock_Threads)
        {
          if (ths.Count > 0)
          {
            foreach (var item in ths)
            {
              if (item.Task != null && item.Task.ID == id)
              {
                task = item.Task;
                var b = item.Running;
                item.Abort(true);
                if (b)
                  state = TaskState.Running;
                else
                  state = TaskState.Finished;
              }
            }
          }
        }
      }
      #endregion

      if (task == null) state = TaskState.Finished;

      // 处理任务结束时的事情
      if (task != null && task.AbortMethod != null)
      {
        try { task.AbortMethod(task.Argument); }
        catch { }
      }

      return state;
    }

    /// <summary>取消所有未开始任务</summary>
    /// <remarks>这里不要调用上面Abort取消单个任务，否则可能会造成死锁</remarks>
    public void AbortAllTask()
    {
      // 重点：
      // 这里使用了锁，很危险，所以仅仅在锁里面删除任务，任务的善后处理在锁外面完成

      if (Tasks.Count < 1) return;
      List<ThreadTask> list = null;
      lock (Sync_Tasks)
      {
        if (Tasks.Count < 1) return;

        list = new List<ThreadTask>();
        foreach (var item in Tasks.Values)
        {
          list.Add(item);
        }
        Tasks.Clear();
      }

      if (list == null || list.Count < 1) return;

      foreach (var item in list)
      {
        if (item.AbortMethod != null)
        {
          try { item.AbortMethod(item.Argument); }
          catch { }
        }
      }
    }

    /// <summary>取消所有进行中任务</summary>
    /// <remarks>这里不要调用上面Abort取消单个任务，否则可能会造成死锁</remarks>
    public void AbortAllThread()
    {
      // 重点：
      // 这里使用了锁，很危险，所以仅仅在锁里面删除任务，任务的善后处理在锁外面完成

      var ths = Threads;
      if (ths.Count < 1) return;
      List<ThreadTask> list = null;
      lock (SyncLock_Threads)
      {
        if (ths.Count < 1) return;

        list = new List<ThreadTask>();
        foreach (var item in ths)
        {
          if (item.Running)
          {
            list.Add(item.Task);
            item.Abort(true);
          }
        }
      }

      if (list == null || list.Count < 1) return;

      foreach (var item in list)
      {
        if (item.AbortMethod != null)
        {
          try { item.AbortMethod(item.Argument); }
          catch { }
        }
      }
    }

    /// <summary>取消所有任务</summary>
    /// <remarks>这里不要调用上面Abort取消单个任务，否则可能会造成死锁</remarks>
    public void AbortAll()
    {
      AbortAllTask();
      AbortAllThread();
    }

    /// <summary>查询任务状态</summary>
    /// <param name="id">任务编号</param>
    /// <returns>任务状态</returns>
    public TaskState Query(Int32 id)
    {
      if (Tasks.Count < 1) return TaskState.Unstarted;

      //检查任务是否还在队列里面
      if (Tasks.ContainsKey(id)) return TaskState.Unstarted;

      //检查任务是否正在处理
      var ths = Threads;
      if (ths.Count < 1) return TaskState.Finished;
      lock (SyncLock_Threads)
      {
        if (ths.Count < 1) return TaskState.Finished;
        foreach (var item in ths)
        {
          if (item.Task != null && item.Task.ID == id)
          {
            if (item.Running)
              return TaskState.Running;
            else
              return TaskState.Finished;
          }
        }
      }
      return TaskState.Finished;
    }

    /// <summary>查询任务个数</summary>
    /// <returns></returns>
    public Int32 QueryCount()
    {
      lock (Sync_Tasks)
      {
        return Tasks.Count;
      }
    }

    /// <summary>等待所有任务完成，并指定是否在等待之前退出同步域。</summary>
    /// <param name="millisecondsTimeout"></param>
    /// <returns>是否在等待之前退出同步域</returns>
    public Boolean WaitAll(Int32 millisecondsTimeout)
    {
      var sw = Stopwatch.StartNew();

      while (true)
      {
        if (RunningCount < 1)
        {
          try
          {
            if (Tasks.Count < 1) break;
          }
          catch (Exception ex)
          {
            WriteLog("取任务数异常！" + ex.ToString());
          }
        }
        if (sw.ElapsedMilliseconds >= millisecondsTimeout) return false;

        if (Interval > 0) Thread.Sleep(Interval);
      }
      return true;
    }
    #endregion

    #region 维护
    /// <summary>调度包装</summary>
    private void Work()
    {
      while (true)
      {
        try
        {
          //等待事件量，超时1秒
          Event.WaitOne(1000, false);
          Event.Reset();

          lock (SyncLock_Threads)
          {
            #region 线程维护与统计
            var ths = Threads;
            Int32 freecount = 0;
            //清理死线程
            for (int i = ths.Count - 1; i >= 0; i--)
            {
              if (ths[i] == null)
              {
                ths.RemoveAt(i);
                if(s_logger.IsInformationLevelEnabled()) s_logger.LogInformation(Name + "线程池的线程对象为空，设计错误！");
              }
              else if (!ths[i].IsAlive)
              {
                ths[i].Dispose();
                if (s_logger.IsInformationLevelEnabled()) s_logger.LogInformation(ths[i].Name + "处于非活动状态，设计错误！");
                ths.RemoveAt(i);
              }
              else if (!ths[i].Running)
                freecount++;
            }
            //正在处理任务的线程数
            RunningCount = ths.Count - freecount;

            WriteLog("总数：" + ths.Count + "  可用：" + freecount + " 任务数：" + Tasks.Count);

            Int32 count = MinThreads - freecount;
            //保留最小线程数个线程
            if (count > 0)
            {
              for (int i = 0; i < count; i++)
              {
                var thread = AddThread();
                if (thread != null) ths.Add(thread);
              }
            }
            else if (count < 0)//过多活动线程，清理不活跃的
            {
              for (int i = ths.Count - 1; i >= 0 && count < 0; i--)
              {
                if (ths[i].CanRelease)
                {
                  ths[i].Dispose();
                  ths.RemoveAt(i);
                  count++;
                }
              }
            }
            #endregion
          }

          //检查任务，分派线程
          if (Tasks.Count > 0)
          {
            lock (Sync_Tasks)
            {
              while (Tasks.Count > 0)
              {
                //借一个线程
                var thread = Open();
                if (thread == null) break;
                WriteLog("借得线程" + thread.Name);

                //拿出一个任务
                Int32 id = Tasks.Keys[0];
                thread.Task = Tasks[id];
                Tasks.RemoveAt(0);

                // 创建线程之间暂停一下
                if (Interval > 0) Thread.Sleep(Interval);

                //处理任务
                thread.Start();
              }
            }
          }
        }
        catch (ThreadInterruptedException ex)
        {
          LastError = ex;
          break;
        }
        catch (ThreadAbortException ex)
        {
          LastError = ex;

          break;
        }
        catch (Exception ex)
        {
          LastError = ex;
          s_logger.LogError(ex.ToString());
        }
      }

      // 结束所有工作了，回家吧
      AbortAll();
    }

    /// <summary>添加线程。本方法不是线程安全，调用者需要自己维护线程安全</summary>
    /// <returns></returns>
    private ThreadX AddThread()
    {
      //保证活动线程数不超过最大线程数
      if (Threads.Count >= MaxThreads) return null;

      var thread = new ThreadX();
      //thread.Name = Name + "线程池" + ThreadCount + "号线程";
      //thread.Name = String.Format("{0}线程池{1,3}号线程", Name, ThreadCount);
      thread.Name = Name + "P" + ThreadCount;
      thread.OnTaskFinished += new EventHandler<EventArgs>(thread_OnTaskFinished);

      ThreadCount++;

      ////暂停一下
      //Thread.Sleep(10);

      WriteLog("新建线程：" + thread.Name);
      return thread;
    }

    void thread_OnTaskFinished(object sender, EventArgs e)
    {
      var thread = sender as ThreadX;

      Close(thread);

      //通知管理线程，任务完成
      Event.Set();
    }
    #endregion

    #region 线程调度
    /// <summary>借用线程</summary>
    /// <returns></returns>
    private ThreadX Open()
    {
      lock (SyncLock_Threads)
      {
        var ths = Threads;
        foreach (var item in ths)
        {
          if (item != null && item.IsAlive && !item.Running) return item;
        }

        //没有空闲线程，加一个
        if (ths.Count < MaxThreads)
        {
          var thread = AddThread();
          ths.Add(thread);

          RunningCount++;

          return thread;
        }
        //else
        //    WriteLog("已达到最大线程数！");
      }
      return null;
    }

    /// <summary>归还线程</summary>
    /// <param name="thread"></param>
    private void Close(ThreadX thread)
    {
      if (thread == null) return;
      WriteLog("归还线程" + thread.Name);

      RunningCount--;

      //看这个线程是活的还是死的，死的需要清除
      if (!thread.IsAlive)
      {
        var ths = Threads;
        if (ths.Contains(thread))
        {
          lock (SyncLock_Threads)
          {
            if (ths.Contains(thread))
            {
              ths.Remove(thread);
              if(s_logger.IsInformationLevelEnabled()) s_logger.LogInformation("归还" + thread.Name + "时发现，线程被关闭了，设计错误！");
            }
          }
        }
        thread.Dispose();
      }
    }
    #endregion

    #region 全局线程池助手
    /// <summary>带异常处理的线程池任务调度</summary>
    /// <param name="callback"></param>
    public static void QueueUserWorkItem(WaitCallback callback) { QueueUserWorkItem(callback, null); }

    /// <summary>带异常处理的线程池任务调度</summary>
    /// <param name="callback"></param>
    /// <param name="state"></param>
    public static void QueueUserWorkItem(WaitCallback callback, Object state) { QueueUserWorkItem(callback, state, ex => s_logger.LogError(ex.ToString())); }

    /// <summary>带异常处理的线程池任务调度，即使不指定异常处理方法，也不允许异常抛出，以免造成应用程序退出</summary>
    /// <param name="callback"></param>
    /// <param name="state"></param>
    /// <param name="errCallback">发生异常时调用的方法</param>
    public static void QueueUserWorkItem(WaitCallback callback, Object state, Action<Exception> errCallback)
    {
      if (callback == null) return;

      var cb = new WaitCallback(s =>
      {
        var ss = (Object[])s;
        var wcb = ss[0] as WaitCallback;
        var st = ss[1];
        var onerr = ss[2] as Action<Exception>;

        try
        {
          wcb(st);
        }
        catch (Exception ex)
        {
          if (onerr != null)
          {
            try { onerr(ex); }
            catch { }
          }
        }
      });

      ThreadPool.QueueUserWorkItem(cb, new Object[] { callback, state, errCallback });
    }

    /// <summary>带异常处理的线程池任务调度</summary>
    /// <param name="callback"></param>
    public static void QueueUserWorkItem(Action callback)
    {
      QueueUserWorkItem(callback, ex =>
      {
        if (s_logger.IsDebugLevelEnabled()) s_logger.LogDebug(ex.ToString());
      });
    }

    /// <summary>带异常处理的线程池任务调度，即使不指定异常处理方法，也不允许异常抛出，以免造成应用程序退出</summary>
    /// <param name="callback"></param>
    /// <param name="errCallback">发生异常时调用的方法</param>
    [DebuggerHidden]
    public static void QueueUserWorkItem(Action callback, Action<Exception> errCallback)
    {
      if (callback == null) return;

      var cb = new WaitCallback(s =>
      {
        var ss = (Object[])s;
        var func = ss[0] as Action;
        var onerr = ss[1] as Action<Exception>;

        try
        {
          func();
        }
        catch (Exception ex)
        {
          if (onerr != null)
          {
            try { onerr(ex); }
            catch { }
          }
        }
      });

      ThreadPool.QueueUserWorkItem(cb, new Object[] { callback, errCallback });
    }
    #endregion

    #region IDisposable 成员
    /// <summary>释放资源</summary>
    public void Dispose()
    {
      Dispose(true);

      GC.SuppressFinalize(this);
    }

    private void Dispose(Boolean disposing)
    {
      WriteLog(Name + "线程池释放资源");
      var ths = Threads;
      if (ths != null && ths.Count > 0)
      {
        lock (SyncLock_Threads)
        {
          if (ths != null && ths.Count > 0)
          {
            for (int i = ths.Count - 1; i >= 0; i--)
            {
              if (ths[i] != null)
              {
                ths[i].Dispose();
              }
              ths.RemoveAt(i);
            }
          }
        }
      }

      if (ManagerThread != null && ManagerThread.IsAlive) ManagerThread.Abort();

      if (_Event != null) _Event.Close();
    }

    /// <summary>析构</summary>
    ~ThreadPoolX()
    {
      Dispose(false);
    }
    #endregion

    #region 辅助函数
    private static ILogger s_logger = TraceLogger.GetLogger("CuteAnt.Threading");
    private static void WriteLog(String msg)
    {
      if (s_logger.IsDebugLevelEnabled()) s_logger.LogDebug("线程：" + Thread.CurrentThread.Name + " 信息：" + msg);
    }

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("{0}线程池，线程数：{1}，任务数：{2}", Name, Threads.Count, Tasks.Count);
    }

    private static Boolean? _Debug;
    /// <summary>是否调试</summary>
    public static Boolean Debug
    {
      get
      {
        if (_Debug != null) { return _Debug.Value; }
        _Debug = SystemConfigs.GetConfig().IsThreadDebug;
        return _Debug.Value;
      }
      set { _Debug = value; }
    }
    #endregion
  }
}