// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System.Buffers;

namespace CuteAnt.IO.Pipelines
{
  /// <summary>The result of a <see cref="IPipeReader.ReadAsync"/> call.</summary>
  public struct ReadResult
  {
    internal ReadOnlyBuffer<byte> ResultBuffer;
    internal ResultFlags ResultFlags;

    public ReadResult(ReadOnlyBuffer<byte> buffer, bool isCancelled, bool isCompleted)
    {
      ResultBuffer = buffer;
      ResultFlags = ResultFlags.None;

      if (isCompleted)
      {
        ResultFlags |= ResultFlags.Completed;
      }
      if (isCancelled)
      {
        ResultFlags |= ResultFlags.Cancelled;
      }
    }

    /// <summary>The <see cref="T:System.Buffers.ReadOnlyBuffer{byte}"/> that was read</summary>
    public ReadOnlyBuffer<byte> Buffer => ResultBuffer;

    /// <summary>True if the currrent read was cancelled</summary>
    public bool IsCancelled => (ResultFlags & ResultFlags.Cancelled) != 0;

    /// <summary>True if the <see cref="IPipeReader"/> is complete</summary>
    public bool IsCompleted => (ResultFlags & ResultFlags.Completed) != 0;
  }
}
#endif
