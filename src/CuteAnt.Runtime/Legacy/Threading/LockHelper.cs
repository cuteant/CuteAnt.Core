// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel\System\ServiceModel\Channels\LockHelper.cs
// </copyright>

using System;
using System.Runtime;
using System.Threading;

namespace CuteAnt.Runtime
{
  // IMPORTANT: Only meant to be used within a using statement.
  public struct LockHelper : IDisposable
  {
    ReaderWriterLockSlim m_readerWriterLock;
    bool m_isReaderLock;
    bool m_isLockHeld;

    LockHelper(ReaderWriterLockSlim readerWriterLock, bool isReaderLock)
    {
      m_readerWriterLock = readerWriterLock;
      m_isReaderLock = isReaderLock;

      if (isReaderLock)
      {
        m_readerWriterLock.EnterReadLock();
      }
      else
      {
        m_readerWriterLock.EnterWriteLock();
      }

      m_isLockHeld = true;
    }

    public void Dispose()
    {
      if (m_isLockHeld)
      {
        m_isLockHeld = false;
        if (m_isReaderLock)
        {
          m_readerWriterLock.ExitReadLock();
        }
        else
        {
          m_readerWriterLock.ExitWriteLock();
        }
      }
    }

    public static IDisposable TakeWriterLock(ReaderWriterLockSlim readerWriterLock)
    {
      Fx.Assert(readerWriterLock != null, "The readerWriterLock cannot be null.");
      return new LockHelper(readerWriterLock, false);
    }

    public static IDisposable TakeReaderLock(ReaderWriterLockSlim readerWriterLock)
    {
      Fx.Assert(readerWriterLock != null, "The readerWriterLock cannot be null.");
      return new LockHelper(readerWriterLock, true);
    }
  }
}
