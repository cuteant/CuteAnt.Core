/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Xml.Serialization;

namespace CuteAnt
{
  /// <summary>具有是否已释放和释放后事件的接口</summary>
  public interface IDisposable2 : IDisposable
  {
    /// <summary>是否已经释放</summary>
    [XmlIgnore]
    Boolean Disposed { get; }

    /// <summary>被销毁时触发事件</summary>
    event EventHandler OnDisposed;
  }

  /// <summary>具有销毁资源处理的抽象基类</summary>
  /// <example>
  /// <code>
  /// /// &lt;summary&gt;子类重载实现资源释放逻辑时必须首先调用基类方法&lt;/summary&gt;
  /// /// &lt;param name="disposing"&gt;从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
  /// /// 因为该方法只会被调用一次，所以该参数的意义不太大。&lt;/param&gt;
  /// protected override void OnDispose(Boolean disposing)
  /// {
  ///     base.OnDispose(disposing);
  ///
  ///     if (disposing)
  ///     {
  ///         // 如果是构造函数进来，不执行这里的代码
  ///     }
  /// }
  /// </code>
  /// </example>
  public abstract class DisposeBase : CriticalFinalizerObject, IDisposable2
  {
    #region 释放资源

    /// <summary>实现IDisposable中的Dispose方法，释放资源</summary>
    public void Dispose()
    {
      // 必须为true
      Dispose(true);
      // 释放托管资源
      // 告诉GC，不要调用析构函数
      GC.SuppressFinalize(this);
    }

    [NonSerialized]
    private Int32 disposed = 0;

    [NonSerialized]
    private Int32 beforeDisposed = 0;

    /// <summary>是否已经释放</summary>
    [XmlIgnore]
    public Boolean Disposed
    {
      get { return disposed > 0; }
    }

    /// <summary>被销毁时触发事件</summary>
    [field: NonSerialized]
    public event EventHandler OnDisposed;

    /// <summary>释放资源，参数表示是否由Dispose调用。该方法保证OnDispose只被调用一次！</summary>
    /// <param name="disposing"></param>
    private void Dispose(Boolean disposing)
    {
      if (disposed != 0) { return; }

      OnBeforeDispose(disposing);

      if (Interlocked.CompareExchange(ref disposed, 1, 0) != 0) { return; }
      //if (HmTrace.Debug)
      //{
      //	try
      //	{
      //		OnDispose(disposing);
      //	}
      //	catch (Exception ex)
      //	{
      //		HmTrace.WriteDebug("设计错误，OnDispose中尽可能的不要抛出异常！{0}", ex.ToString());
      //		throw;
      //	}
      //}
      //else
      //{
      OnDispose(disposing);
      //}

      // 只有基类的OnDispose被调用，才有可能是2
      if (Interlocked.CompareExchange(ref disposed, 3, 2) != 2)
      {
        throw new Exception("设计错误，OnDispose应该只被调用一次！代码不应该直接调用OnDispose，而应该调用Dispose。子类重载OnDispose时必须首先调用基类方法！");
      }
    }

    /// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
    /// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
    /// 因为该方法只会被调用一次，所以该参数的意义不太大。</param>
    protected virtual void OnBeforeDispose(Boolean disposing)
    {
      if (beforeDisposed != 0) { return; }
      if (Interlocked.CompareExchange(ref beforeDisposed, 1, 0) != 0) { return; }
    }

    /// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
    /// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
    /// 因为该方法只会被调用一次，所以该参数的意义不太大。</param>
    protected virtual void OnDispose(Boolean disposing)
    {
      // 只有从Dispose中调用，才有可能是1
      if (Interlocked.CompareExchange(ref disposed, 2, 1) != 1)
      {
        throw new Exception("设计错误，OnDispose应该只被调用一次！代码不应该直接调用OnDispose，而应该调用Dispose。");
      }
      //if (disposing)
      //{
      //	// 释放托管资源
      //	// 告诉GC，不要调用析构函数
      //	GC.SuppressFinalize(this);
      //}

      // 释放非托管资源
      if (OnDisposed != null)
      {
        OnDisposed(this, EventArgs.Empty);
      }
    }

    /// <summary>析构函数</summary>
    /// <remarks>
    /// 必须，以备程序员忘记了显式调用Dispose方法
    /// 如果忘记调用Dispose，这里会释放非托管资源
    /// 如果曾经调用过Dispose，因为GC.SuppressFinalize(this)，不会再调用该析构函数
    /// </remarks>
    ~DisposeBase()
    {
      // 必须为false
      Dispose(false);
    }

    #endregion
  }

  /// <summary>销毁助手。扩展方法专用</summary>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  public static class DisposeHelper
  {
    /// <summary>尝试销毁对象，如果有<see cref="IDisposable"/>则调用</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Object TryDispose(this Object obj)
    {
      if (obj == null) { return obj; }

      #region ## 苦竹 修改 ##
      //// 列表元素销毁
      //if (obj is IEnumerable)
      //{
      //	// 对于枚举成员，先考虑添加到列表，再逐个销毁，避免销毁过程中集合改变
      //	var list = obj as IList;
      //	if (list == null)
      //	{
      //		list = new List<Object>();
      //		foreach (var item in (obj as IEnumerable))
      //		{
      //			if (item is IDisposable) list.Add(item);
      //		}
      //	}
      //	foreach (var item in list)
      //	{
      //		if (item is IDisposable)
      //		{
      //			try
      //			{
      //				//(item as IDisposable).TryDispose();
      //				// 只需要释放一层，不需要递归
      //				// 因为一般每一个对象负责自己内部成员的释放
      //				(item as IDisposable).Dispose();
      //			}
      //			catch { }
      //		}
      //	}
      //}
      //// 对象销毁
      //if (obj is IDisposable)
      //{
      //	try
      //	{
      //		(obj as IDisposable).Dispose();
      //	}
      //	catch { }
      //}

      // 列表元素销毁
      var list = obj as IList;
      if (list == null)
      {
        // 对于枚举成员，先考虑添加到列表，再逐个销毁，避免销毁过程中集合改变
        var ienumObj = obj as IEnumerable;
        if (ienumObj != null)
        {
          list = new List<Object>();
          foreach (var item in ienumObj)
          {
            if (item is IDisposable) { list.Add(item); }
          }
        }
      }
      if (list != null)
      {
        foreach (var item in list)
        {
          var idisItem = item as IDisposable;
          if (idisItem != null)
          {
            try
            {
              //idisItem.TryDispose();
              // 只需要释放一层，不需要递归
              // 因为一般每一个对象负责自己内部成员的释放
              idisItem.Dispose();
            }
            catch { }
          }
        }
      }
      // 对象销毁
      var idisObj = obj as IDisposable;
      if (idisObj != null)
      {
        try
        {
          idisObj.Dispose();
        }
        catch { }
      }
      #endregion

      return obj;
    }
  }
}