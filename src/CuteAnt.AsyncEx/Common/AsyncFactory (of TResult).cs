using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.Reflection;

namespace CuteAnt.AsyncEx
{
	/// <summary>Provides asynchronous wrappers.</summary>
	/// <typeparam name="TResult">The type of the result of the asychronous operation.</typeparam>
	public static partial class AsyncFactory<TResult>
	{
		#region **& Callback &**

		private static AsyncCallback Callback(Func<IAsyncResult, TResult> endMethod, TaskCompletionSource<TResult> tcs)
		{
			return asyncResult =>
			{
				try
				{
					tcs.TrySetResult(endMethod(asyncResult));
				}
				catch (OperationCanceledException)
				{
					tcs.TrySetCanceled();
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			};
		}

		#endregion

		#region --& FromApm &--

		/// <summary>Wraps a begin/end asynchronous method.</summary>
		/// <param name="beginMethod">The begin method. May not be <c>null</c>.</param>
		/// <param name="endMethod">The end method. May not be <c>null</c>.</param>
		/// <returns>The result of the asynchronous operation.</returns>
		public static Task<TResult> FromApm(Func<AsyncCallback, Object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod)
		{
			var tcs = new TaskCompletionSource<TResult>();
			beginMethod(Callback(endMethod, tcs), null);
			return tcs.Task;
		}

		#endregion

		#region --& ToBegin &--

		/// <summary>Wraps a <see cref="Task{TResult}"/> into the Begin method of an APM pattern.</summary>
		/// <param name="task">The task to wrap. May not be <c>null</c>.</param>
		/// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
		/// <param name="state">The state passed into the Begin method of the APM pattern.</param>
		/// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
		public static IAsyncResult ToBegin(Task<TResult> task, AsyncCallback callback, Object state)
		{
			var tcs = new TaskCompletionSource<TResult>(state);
#if !NET40
			task.ContinueWith((t, s) =>
			{
				var tuple = (Tuple<TaskCompletionSource<TResult>, AsyncCallback>)s;
				var tcs1 = tuple.Item1;
				var callback1 = tuple.Item2;

				tcs1.TryCompleteFromCompletedTask(t);
				if (callback1 != null) { callback1(tcs1.Task); }
			}, Tuple.Create(tcs, callback), CancellationToken.None, AsyncUtils.GetContinuationOptions(), TaskScheduler.Default);
#else
			task.ContinueWith(t =>
			{
				tcs.TryCompleteFromCompletedTask(t);

				if (callback != null) { callback(tcs.Task); }
			}, CancellationToken.None, AsyncUtils.GetContinuationOptions(), TaskScheduler.Default);
#endif

			return tcs.Task;
		}

		#endregion

		#region --& ToEnd &--

		/// <summary>Wraps a <see cref="Task{TResult}"/> into the End method of an APM pattern.</summary>
		/// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
		/// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
		public static TResult ToEnd(IAsyncResult asyncResult)
		{
			return ((Task<TResult>)asyncResult).WaitAndUnwrapException();
		}

		#endregion

		#region --& FromEvent &--

		/// <summary>Gets a task that will complete the next time an event is raised. 
		/// The event type must follow the standard <c>void EventHandlerType(Object, TResult)</c> pattern. 
		/// Be mindful of race conditions (i.e., if the event is raised immediately before this method is called, your task may never complete).</summary>
		/// <param name="target">The object that publishes the event.</param>
		/// <returns>The event args.</returns>
		public static Task<TResult> FromEvent(Object target)
		{
			// Try to look up an event that has the same name as the TResult type (stripping the trailing "EventArgs").
			var type = target.GetType();
			var resultType = typeof(TResult);
			var resultName = resultType.Name;
			if (resultName.EndsWith("EventArgs", StringComparison.Ordinal))
			{
				var eventInfo = type.GetTypeInfo().GetDeclaredEvent(resultName.Remove(resultName.Length - 9));
				if (eventInfo != null)
				{
					return new EventArgsTask<TResult>(target, eventInfo).Task;
				}
			}

			// Try to match to any event with the correct signature.
			EventInfo match = null;
			foreach (var eventInfo in type.GetTypeInfo().DeclaredEvents)
			{
				var invoke = eventInfo.EventHandlerType.GetTypeInfo().GetDeclaredMethod("Invoke");
				if (invoke.ReturnType != typeof(void)) { continue; }
				var parameters = invoke.GetParameters();
				if (parameters.Length != 2 || parameters[0].ParameterType != typeof(Object) || parameters[1].ParameterType != resultType) { continue; }

				if (match != null) { throw new InvalidOperationException("Found multiple matching events on type " + target.GetType().FullName); }
				match = eventInfo;
			}

			if (match == null) { throw new InvalidOperationException("Could not find a matching event on type " + target.GetType().FullName); }
			return new EventArgsTask<TResult>(target, match).Task;
		}

		#endregion

		#region --& FromEvent &--

		/// <summary>Gets a task that will complete the next time an event is raised. 
		/// The event type must follow the standard <c>void EventHandlerType(Object, TResult)</c> pattern. 
		/// Be mindful of race conditions (i.e., if the event is raised immediately before this method is called, 
		/// your task may never complete).</summary>
		/// <param name="target">The object that publishes the event. May not be <c>null</c>.</param>
		/// <param name="eventName">The name of the event. May not be <c>null</c>.</param>
		/// <returns>The event args.</returns>
		public static Task<TResult> FromEvent(Object target, String eventName)
		{
			var eventInfo = target.GetType().GetTypeInfo().GetDeclaredEvent(eventName);
			if (eventInfo == null)
			{
				throw new InvalidOperationException("Could not find event " + eventName + " on type " + target.GetType().FullName);
			}

			return new EventArgsTask<TResult>(target, eventInfo).Task;
		}

		#endregion

		#region ** class EventArgsTask<TEventArgs> **

		/// <summary>Manages the subscription to an event on a target object, 
		/// triggering a task (and unsubscribing) when the event is raised.</summary>
		/// <typeparam name="TEventArgs">The type of event arguments passed to the event.</typeparam>
		private sealed class EventArgsTask<TEventArgs>
		{
			/// <summary>The source for our task, which is returned to the user.</summary>
			private readonly TaskCompletionSource<TEventArgs> _tcs;

			/// <summary>The subscription to the event.</summary>
			private readonly Delegate _subscription;

			/// <summary>The object that publishes the event.</summary>
			private readonly Object _target;

			/// <summary>The event to which we subscribe.</summary>
			private readonly EventInfo _eventInfo;

			/// <summary>Subscribes to the specified event.</summary>
			/// <param name="target">The object that publishes the event.</param>
			/// <param name="eventInfo">The event to which we subscribe.</param>
			public EventArgsTask(Object target, EventInfo eventInfo)
			{
				_tcs = new TaskCompletionSource<TEventArgs>();
				_target = target;
				_eventInfo = eventInfo;
				var eventCompletedMethod = GetType().GetTypeInfo().GetDeclaredMethod("EventCompleted");
				_subscription = eventCompletedMethod.CreateDelegate(eventInfo.EventHandlerType, this);
				eventInfo.AddEventHandler(target, _subscription);
			}

			/// <summary>Gets the task that is completed when the event is raised.</summary>
			public Task<TEventArgs> Task { get { return _tcs.Task; } }

			// ReSharper disable UnusedMember.Local
			// ReSharper disable UnusedParameter.Local
			/// <summary>Private method that handles event completion. Do not call this method; 
			/// it is public to avoid security problems when reflecting.</summary>
			public void EventCompleted(Object sender, TEventArgs args)
			{
				_eventInfo.RemoveEventHandler(_target, _subscription);
				var asyncArgs = args as AsyncCompletedEventArgs;
				if (asyncArgs != null)
				{
					if (asyncArgs.Cancelled)
					{
						_tcs.TrySetCanceled();
					}
					else if (asyncArgs.Error != null)
					{
						_tcs.TrySetException(asyncArgs.Error);
					}
				}

				_tcs.TrySetResult(args);
			}

			// ReSharper restore UnusedParameter.Local
			// ReSharper restore UnusedMember.Local
		}

		#endregion
	}
}