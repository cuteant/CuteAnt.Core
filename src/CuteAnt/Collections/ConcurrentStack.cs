﻿#if NET_2_0
/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.Threading;

namespace System.Collections.Concurrent
{
	/// <summary>先进先出LIFO的原子栈结构，采用CAS保证线程安全。利用单链表实现。</summary>
	/// <remarks>
	/// 注意：<see cref="Push"/>、<see cref="TryPop"/>、<see cref="Pop"/>、<see cref="TryPeek"/>、<see cref="Peek"/>是重量级线程安全代码，不要随意更改。
	///
	/// 增加自由节点链表，避免频繁分配节点带来的GC压力。
	///
	/// 经过测试，对象数量在万级以上时，性能急剧下降！
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	public class ConcurrentStack<T> : DisposeBase, IStack<T>, IEnumerable<T>, ICollection, IEnumerable
	{
		#region 属性

		/// <summary>栈顶</summary>
		private SingleListNode<T> Top;

		private Int32 _Count;

		/// <summary>元素个数</summary>
		public Int32 Count { get { return _Count; } }

		/// <summary>是否空</summary>
		public Boolean IsEmpty { get { return Top == null; } }

		private Boolean _UseNodePool;

		/// <summary>是否使用节点池。采用节点池可以避免分配节点造成的GC压力，但是会让性能有所降低。</summary>
		public Boolean UseNodePool { get { return _UseNodePool; } set { _UseNodePool = value; } }

		#endregion

		#region 构造

		/// <summary>实例化</summary>
		public ConcurrentStack()
		{
		}

		/// <summary>使用一个集合初始化</summary>
		/// <param name="collection"></param>
		public ConcurrentStack(IEnumerable<T> collection)
		{
			SingleListNode<T> node = null;
			foreach (T local in collection)
			{
				var node2 = new SingleListNode<T>(local);
				node2.Next = node;
				node = node2;
			}
			Top = node;
		}

		/// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
		/// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）</param>
		protected override void OnDispose(bool disposing)
		{
			base.OnDispose(disposing);

			Clear();
			Top = null;
		}

		#endregion

		#region 核心方法

		/// <summary>向栈压入一个对象</summary>
		/// <remarks>重点解决多线程环境下资源争夺以及使用lock造成性能损失的问题</remarks>
		/// <param name="item"></param>
		public void Push(T item)
		{
			Debug.Assert(item != null);

			var node = CreateNode(item);
			// 设置新对象的下一个节点为当前栈顶
			node.Next = Top;
			if (Interlocked.CompareExchange(ref Top, node, node.Next) != node.Next) this.PushCore(node, node);

			Interlocked.Increment(ref _Count);

			// 数量较多时，自动采用节点池
			if (!UseNodePool && _Count > 100) UseNodePool = true;
		}

		/// <summary>向栈压入一个对象数组</summary>
		/// <param name="items"></param>
		/// <param name="startIndex"></param>
		/// <param name="count"></param>
		public void PushRange(T[] items, int startIndex = 0, int count = -1)
		{
			if (items == null) throw new ArgumentNullException("items");
			if (count < 0) count = items.Length - startIndex;

			if (count == 0) return;

			var head = CreateNode(items[startIndex]);
			var tail = head;
			for (int i = startIndex + 1; i < startIndex + count; i++)
			{
				var node = CreateNode(items[i]);
				node.Next = head;
				head = node;
			}
			tail.Next = Top;
			if (Interlocked.CompareExchange(ref Top, head, tail.Next) != tail.Next) this.PushCore(head, tail);

			Interlocked.Add(ref _Count, count);

			// 数量较多时，自动采用节点池
			if (!UseNodePool && _Count > 100) UseNodePool = true;
		}

		private void PushCore(SingleListNode<T> head, SingleListNode<T> tail)
		{
			var wait = new SpinWait();
			do
			{
				wait.SpinOnce();

				// 设置新对象的下一个节点为当前栈顶
				tail.Next = Top;
			}
			// 比较并交换
			// 如果当前栈顶第一个参数的Top等于第三个参数，表明没有被别的线程修改，保存第二参数到第一参数中
			// 否则，不相等表明当前栈顶已经被修改过，操作失败，执行循环
			while (Interlocked.CompareExchange(ref Top, head, tail.Next) != tail.Next);
		}

		/// <summary>从栈中弹出一个对象</summary>
		/// <returns></returns>
		public T Pop()
		{
			T item;
			if (!TryPop(out item)) throw new InvalidOperationException("栈为空！");

			return item;
		}

		/// <summary>尝试从栈中弹出一个对象</summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public Boolean TryPop(out T item)
		{
			SingleListNode<T> newTop;
			SingleListNode<T> oldTop;
			//SpinWait sw = null;
			while (true)
			{
				// 记住当前栈顶
				oldTop = Top;
				if (oldTop == null)
				{
					item = default(T);
					return false;
				}
				Debug.Assert(_Count > 0);

				// 设置新栈顶为当前栈顶的下一个节点
				newTop = oldTop.Next;

				// 比较并交换
				// 如果当前栈顶第一个参数的Top等于第三个参数，表明没有被别的线程修改，保存第二参数到第一参数中
				// 否则，不相等表明当前栈顶已经被修改过，操作失败，执行循环
				if (Interlocked.CompareExchange(ref Top, newTop, oldTop) == oldTop) break;

				Thread.SpinWait(1);
				//if (sw == null) sw = new SpinWait();
				//sw.SpinOnce();
			}

			Interlocked.Decrement(ref _Count);

			item = oldTop.Item;
			Debug.Assert(item != null);

			if (UseNodePool)
				PushNode(oldTop);
			else
			{
				// 断开关系链，避免内存泄漏
				oldTop.Next = null;
				oldTop.Item = default(T);
			}

			return true;
		}

		/// <summary>获取栈顶对象，不弹栈</summary>
		/// <returns></returns>
		public T Peek()
		{
			T item;
			if (!TryPeek(out item)) throw new InvalidOperationException("栈为空！");

			return item;
		}

		/// <summary>尝试获取栈顶对象，不弹栈</summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public Boolean TryPeek(out T item)
		{
			var top = Top;
			if (top == null)
			{
				item = default(T);
				return false;
			}
			item = top.Item;
			return true;
		}

		#endregion

		#region 节点池

		/// <summary>自由节点头部</summary>
		private SingleListNode<T> FreeTop;

		private Int32 FreeCount;

		private const Int32 MaxFreeCount = 100;

		private SingleListNode<T> CreateNode(T item)
		{
			return UseNodePool ? PopNode(item) : new SingleListNode<T>(item);
		}

		private SingleListNode<T> PopNode(T item)
		{
			SingleListNode<T> newTop;
			SingleListNode<T> oldTop;
			//SpinWait sw = null;
			while (true)
			{
				// 记住当前栈顶
				oldTop = FreeTop;
				if (oldTop == null) return new SingleListNode<T>(item);

				// 设置新栈顶为当前栈顶的下一个节点
				newTop = oldTop.Next;

				if (Interlocked.CompareExchange(ref FreeTop, newTop, oldTop) == oldTop) break;

				Thread.SpinWait(1);
				//if (sw == null) sw = new SpinWait();
				//sw.SpinOnce();
			}

			Interlocked.Decrement(ref FreeCount);

			oldTop.Next = null;
			oldTop.Item = item;

			return oldTop;
		}

		private void PushNode(SingleListNode<T> node)
		{
			// 断开关系链，避免内存泄漏
			node.Item = default(T);

			//// 如果自由节点太多，就不要了
			//if (FreeCount > MaxFreeCount) return;

			var newTop = node;
			//SpinWait sw = null;
			while (true)
			{
				// 记住当前
				var oldTop = FreeTop;

				// 设置新对象的下一个节点为当前栈顶
				newTop.Next = oldTop;

				if (Interlocked.CompareExchange(ref FreeTop, newTop, oldTop) == oldTop) break;

				Thread.SpinWait(1);
				//if (sw == null) sw = new SpinWait();
				//sw.SpinOnce();
			}

			Interlocked.Increment(ref FreeCount);
		}

		#endregion

		#region 集合方法

		/// <summary>清空</summary>
		public void Clear()
		{
			var top = Top;
			_Count = 0;
			Top = null;

			for (var node = top; node != null; node = node.Next)
			{
				if (UseNodePool)
					PushNode(node);
				else
				{
					// 断开关系链，避免内存泄漏
					node.Next = null;
					node.Item = default(T);
				}
			}
		}

		/// <summary>转为数组</summary>
		/// <returns></returns>
		public T[] ToArray()
		{
			return ToList().ToArray();
		}

		private List<T> ToList()
		{
			var list = new List<T>();
			for (var node = Top; node != null; node = node.Next)
			{
				list.Add(node.Item);
			}
			return list;
		}

		#endregion

		#region ICollection 成员

		void ICollection.CopyTo(Array array, int index)
		{
			if (Top == null || array == null || index >= array.Length) return;

			for (var node = Top; node != null && index < array.Length; node = node.Next) array.SetValue(node.Item, index++);
		}

		bool ICollection.IsSynchronized { get { return true; } }

		private Object _syncRoot;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		#endregion

		#region IEnumerable 成员

		/// <summary>获取枚举器</summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (var node = Top; node != null; node = node.Next) yield return node.Item;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}

#endif