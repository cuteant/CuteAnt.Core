// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CuteAnt.Diagnostics;

namespace CuteAnt
{
  internal sealed class InternalSR
  {
    internal readonly static string ActionItemIsAlreadyScheduled = "ActionItemIsAlreadyScheduled";
    internal readonly static string AsyncCallbackThrewException = "AsyncCallbackThrewException";
    internal readonly static string AsyncResultAlreadyEnded = "AsyncResultAlreadyEnded";
    internal readonly static string BadCopyToArray = "BadCopyToArray";
    internal readonly static string BufferIsNotRightSizeForBufferManager = "BufferIsNotRightSizeForBufferManager";
    internal readonly static string DictionaryIsReadOnly = "DictionaryIsReadOnly";
    internal readonly static string InvalidAsyncResult = "InvalidAsyncResult";
    internal readonly static string InvalidAsyncResultImplementationGeneric = "InvalidAsyncResultImplementationGeneric";
    internal readonly static string InvalidNullAsyncResult = "InvalidNullAsyncResult";
    internal readonly static string InvalidSemaphoreExit = "InvalidSemaphoreExit";
    internal readonly static string KeyCollectionUpdatesNotAllowed = "KeyCollectionUpdatesNotAllowed";
    internal readonly static string KeyNotFoundInDictionary = "KeyNotFoundInDictionary";
    internal readonly static string MustCancelOldTimer = "MustCancelOldTimer";
    internal readonly static string NullKeyAlreadyPresent = "NullKeyAlreadyPresent";
    internal readonly static string ReadNotSupported = "Read not supported on this stream.";
    internal readonly static string SeekNotSupported = "Seek not supported on this stream.";
    internal readonly static string SFxTaskNotStarted = "SFxTaskNotStarted";
    internal readonly static string ThreadNeutralSemaphoreAborted = "ThreadNeutralSemaphoreAborted";
    internal readonly static string ValueCollectionUpdatesNotAllowed = "ValueCollectionUpdatesNotAllowed";
    internal readonly static string ValueMustBeNonNegative = "The value of this argument must be non-negative.";
    internal readonly static string SFxCloseTimedOut1 = "The ServiceHost close operation timed out after {0}.  This could be because a client failed to close a sessionful channel within the required time.  The time allotted to this operation may have been a portion of a longer timeout.";
    internal readonly static string ValueMustBeInRange = "The value of this argument must fall within the range {0} to {1}.";
    internal readonly static string SynchronizedCollectionWrongTypeNull = "A null value cannot be added to the generic collection, because the collection has been parameterized with a value type.";
    internal readonly static string SynchronizedCollectionWrongType1 = "A value of type '{0}' cannot be added to the generic collection, because the collection has been parameterized with a different type.";
    internal readonly static string CannotAddTwoItemsWithTheSameKeyToSynchronizedKeyedCollection0 = "Cannot add two items with the same key to SynchronizedKeyedCollection.";
    internal readonly static string ItemDoesNotExistInSynchronizedKeyedCollection0 = "Item does not exist in SynchronizedKeyedCollection.";
    internal readonly static string SFxCollectionReadOnly = "This operation is not supported because the collection is read-only.";
    internal readonly static string SFxCopyToRequiresICollection = "SynchronizedReadOnlyCollection's CopyTo only works if the underlying list implements ICollection.";
    internal readonly static string SFxCollectionWrongType2 = "The collection of type {0} does not support values of type {1}.";
    internal readonly static string WriterAsyncWritePending = "An asynchronous write is pending on the stream. Ensure that there are no uncompleted asynchronous writes before attempting the next write.";
    internal readonly static string NoAsyncWritePending = "There is no pending asynchronous write on this stream. Ensure that there is pending write on the stream or verify that the implementation does not try to complete the same operation multiple times.";
    internal readonly static string StreamClosed = "The operation cannot be completed because the stream is closed.";
    internal readonly static string WriteAsyncWithoutFreeBuffer = "An asynchronous write was called on the stream without a free buffer.";
    internal readonly static string FlushBufferAlreadyInUse = "Cannot write to a buffer which is currently being flushed.";
    internal readonly static string _BufferedOutputStreamQuotaExceeded = "The size quota for this stream ({0}) has been exceeded.";

    internal static string ArgumentNullOrEmpty(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string AsyncEventArgsCompletedTwice(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string AsyncEventArgsCompletionPending(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string AsyncResultCompletedTwice(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string BufferAllocationFailed(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string BufferedOutputStreamQuotaExceeded(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string CannotConvertObject(object param0, object param1) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string EtwAPIMaxStringCountExceeded(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string EtwMaxNumberArgumentsExceeded(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string EtwRegistrationFailed(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string FailFastMessage(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string IncompatibleArgumentType(object param0, object param1) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string InvalidAsyncResultImplementation(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string LockTimeoutExceptionMessage(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string ShipAssertExceptionMessage(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string TaskTimedOutError(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string TimeoutInputQueueDequeue(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string TimeoutMustBeNonNegative(object param0, object param1) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string TimeoutMustBePositive(object param0, object param1) { throw ExceptionHelper.PlatformNotSupported(); }
    internal static string TimeoutOnOperation(object param0) { throw ExceptionHelper.PlatformNotSupported(); }
  }
}
