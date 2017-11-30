﻿using System;
using NUnit.Framework;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

#if NET40
#if NO_ENLIGHTENMENT
namespace Tests_NET4_NE
#else
namespace Tests_NET4
#endif
#else
#if NO_ENLIGHTENMENT
namespace Tests_NE
#else
namespace Tests
#endif
#endif
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class TaskCompletionSourceUnitTests
    {
        [Test]
        public void ConstructorWithState_SetsAsyncState()
        {
            var state = new object();
            var tcs = new TaskCompletionSource(state);
            Assert.AreSame(state, tcs.Task.AsyncState);
        }

        [Test]
        public void ConstructorWithOptions_SetsOptions()
        {
            var options = TaskCreationOptions.AttachedToParent;
            var tcs = new TaskCompletionSource(options);
            Assert.AreEqual(options, tcs.Task.CreationOptions);
        }

        [Test]
        public void ConstructorWithStateAndOptions_SetsAsyncStateAndOptions()
        {
            var state = new object();
            var options = TaskCreationOptions.AttachedToParent;
            var tcs = new TaskCompletionSource(state, options);
            Assert.AreSame(state, tcs.Task.AsyncState);
            Assert.AreEqual(options, tcs.Task.CreationOptions);
        }

        [Test]
        public void SetCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource();
            tcs.SetCanceled();
            Assert.IsTrue(tcs.Task.IsCanceled);
        }

        [Test]
        public void TrySetCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource();
            tcs.TrySetCanceled();
            Assert.IsTrue(tcs.Task.IsCanceled);
        }

        [Test]
        public void SetException_FaultsTask()
        {
            var e = new InvalidOperationException();
            var tcs = new TaskCompletionSource();
            tcs.SetException(e);
            Assert.IsTrue(tcs.Task.IsFaulted);
            Assert.AreSame(e, tcs.Task.Exception.InnerException);
        }

        [Test]
        public void TrySetException_FaultsTask()
        {
            var e = new InvalidOperationException();
            var tcs = new TaskCompletionSource();
            tcs.TrySetException(e);
            Assert.IsTrue(tcs.Task.IsFaulted);
            Assert.AreSame(e, tcs.Task.Exception.InnerException);
        }

        [Test]
        public void SetExceptions_FaultsTask()
        {
            var e = new[] { new InvalidOperationException() };
            var tcs = new TaskCompletionSource();
            tcs.SetException(e);
            Assert.IsTrue(tcs.Task.IsFaulted);
            Assert.IsTrue(tcs.Task.Exception.InnerExceptions.SequenceEqual(e));
        }

        [Test]
        public void TrySetExceptions_FaultsTask()
        {
            var e = new[] { new InvalidOperationException() };
            var tcs = new TaskCompletionSource();
            tcs.TrySetException(e);
            Assert.IsTrue(tcs.Task.IsFaulted);
            Assert.IsTrue(tcs.Task.Exception.InnerExceptions.SequenceEqual(e));
        }

        [Test]
        public void SetResult_CompletesTask()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.SetResult();
                await tcs.Task;
            });
        }

        [Test]
        public void TrySetResult_CompletesTask()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TrySetResult();
                await tcs.Task;
            });
        }
    }
}
