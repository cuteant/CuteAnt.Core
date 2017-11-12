// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CuteAnt.Diagnostics;

namespace CuteAnt
{
  internal sealed class InternalSR
  {
    public const string ActionItemIsAlreadyScheduled = "Action Item Is Already Scheduled";
    public const string AsyncCallbackThrewException = "Async Callback Threw Exception";
    public const string AsyncResultAlreadyEnded = "Async Result Already Ended";
    public const string BadCopyToArray = "Bad Copy To Array";
    public const string BufferIsNotRightSizeForBufferManager = "Buffer Is Not Right Size For Buffer Manager";
    public const string DictionaryIsReadOnly = "Dictionary Is Read Only";
    public const string InvalidAsyncResult = "Invalid Async Result";
    public const string InvalidAsyncResultImplementationGeneric = "Invalid Async Result Implementation Generic";
    public const string InvalidNullAsyncResult = "Invalid Null Async Result";
    public const string InvalidSemaphoreExit = "Invalid Semaphore Exit";
    public const string KeyCollectionUpdatesNotAllowed = "Key Collection Updates Not Allowed";
    public const string KeyNotFoundInDictionary = "Key Not Found In Dictionary";
    public const string MustCancelOldTimer = "Must Cancel Old Timer";
    public const string NullKeyAlreadyPresent = "Null Key Already Present";
    public const string ReadNotSupported = "Read not supported on this stream.";
    public const string SFxTaskNotStarted = "SFx Task Not Started";
    public const string SeekNotSupported = "Seek not supported on this stream.";
    public const string ThreadNeutralSemaphoreAborted = "Thread Neutral Semaphore Aborted";
    public const string ValueCollectionUpdatesNotAllowed = "Value Collection Updates Not Allowed";
    public const string ValueMustBeNonNegative = "The value of this argument must be non-negative.";

    public const string SFxCloseTimedOut1 = "The ServiceHost close operation timed out after {0}.  This could be because a client failed to close a sessionful channel within the required time.  The time allotted to this operation may have been a portion of a longer timeout.";
    public const string ValueMustBeInRange = "The value of this argument must fall within the range {0} to {1}.";
    public const string SynchronizedCollectionWrongTypeNull = "A null value cannot be added to the generic collection, because the collection has been parameterized with a value type.";
    public const string SynchronizedCollectionWrongType1 = "A value of type '{0}' cannot be added to the generic collection, because the collection has been parameterized with a different type.";
    public const string CannotAddTwoItemsWithTheSameKeyToSynchronizedKeyedCollection0 = "Cannot add two items with the same key to SynchronizedKeyedCollection.";
    public const string ItemDoesNotExistInSynchronizedKeyedCollection0 = "Item does not exist in SynchronizedKeyedCollection.";
    public const string SFxCollectionReadOnly = "This operation is not supported because the collection is read-only.";
    public const string SFxCopyToRequiresICollection = "SynchronizedReadOnlyCollection's CopyTo only works if the underlying list implements ICollection.";
    public const string SFxCollectionWrongType2 = "The collection of type {0} does not support values of type {1}.";
    public const string WriterAsyncWritePending = "An asynchronous write is pending on the stream. Ensure that there are no uncompleted asynchronous writes before attempting the next write.";
    public const string NoAsyncWritePending = "There is no pending asynchronous write on this stream. Ensure that there is pending write on the stream or verify that the implementation does not try to complete the same operation multiple times.";
    public const string StreamClosed = "The operation cannot be completed because the stream is closed.";
    public const string WriteAsyncWithoutFreeBuffer = "An asynchronous write was called on the stream without a free buffer.";
    public const string FlushBufferAlreadyInUse = "Cannot write to a buffer which is currently being flushed.";
    public const string _BufferedOutputStreamQuotaExceeded = "The size quota for this stream ({0}) has been exceeded.";
    public const string DnsResolveFailed = "No DNS entries exist for host {0}.";

    public static string ArgumentNullOrEmpty(string paramName)
    {
      return string.Format("{0} is null or empty");
    }

    public static string AsyncEventArgsCompletedTwice(Type t)
    {
      return string.Format("AsyncEventArgs completed twice for {0}", t);
    }

    public static string AsyncEventArgsCompletionPending(Type t)
    {
      return string.Format("AsyncEventArgs completion pending for {0}", t);
    }

    public static string BufferAllocationFailed(int size)
    {
      return string.Format("Buffer allocation of size {0} failed", size);
    }

    public static string BufferedOutputStreamQuotaExceeded(int maxSizeQuota)
    {
      return string.Format("Buffered output stream quota exceeded (maxSizeQuota={0})", maxSizeQuota);
    }

    public static string CannotConvertObject(object source, Type t)
    {
      return string.Format("Cannot convert object {0} to {1}", source, t);
    }

    public static string EtwAPIMaxStringCountExceeded(object max)
    {
      return string.Format("ETW API max string count exceeded {0}", max);
    }

    public static string EtwMaxNumberArgumentsExceeded(object max)
    {
      return string.Format("ETW max number arguments exceeded {0}", max);
    }

    public static string EtwRegistrationFailed(object arg)
    {
      return string.Format("ETW registration failed {0}", arg);
    }

    public static string FailFastMessage(string description)
    {
      return string.Format("Fail fast: {0}", description);
    }

    public static string InvalidAsyncResultImplementation(Type t)
    {
      return string.Format("Invalid AsyncResult implementation: {0}", t);
    }

    public static string LockTimeoutExceptionMessage(object timeout)
    {
      return string.Format("Lock timeout {0}", timeout);
    }

    public static string ShipAssertExceptionMessage(object description)
    {
      return string.Format("Ship assert exception {0}", description);
    }

    public static string TaskTimedOutError(object timeout)
    {
      return string.Format("Task timed out error {0}", timeout);
    }

    public static string TimeoutInputQueueDequeue(object timeout)
    {
      return string.Format("Timeout input queue dequeue {0}", timeout);
    }

    public static string TimeoutMustBeNonNegative(object argumentName, object timeout)
    {
      return string.Format("Timeout must be non-negative {0} and {1}", argumentName, timeout);
    }

    public static string TimeoutMustBePositive(string argumentName, object timeout)
    {
      return string.Format("Timeout must be positive {0} {1}", argumentName, timeout);
    }

    public static string TimeoutOnOperation(object timeout)
    {
      return string.Format("Timeout on operation {0}", timeout);
    }

    public static string AsyncResultCompletedTwice(Type t)
    {
      return string.Format("AsyncResult Completed Twice for {0}", t);
    }
  }
}
