using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using CuteAnt.Runtime;
//using Nessos.LinqOptimizer.CSharp;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Buffers
{
    /// <summary>The default buffer segment list stream</summary>
    public class BufferedByteArrayStream : Stream, IBufferedStreamCloneable, IBufferedStream
    {
        private IList<ArraySegment<Byte>> m_segments;
        private Int64 m_position;
        private ArraySegment<Byte> m_currentSegment;
        private Int32 m_currentSegmentIndex;
        private Int32 m_currentSegmentOffset;
        private Int64 m_currentSegmentPosition;
        private Int64 m_length;

        private ArrayPool<byte> m_bufferManager;

        private const Int32 c_off = 0;
        private const Int32 c_on = 1;
        private Int32 m_isDisposed = c_off;

        private bool m_callerReturnsBuffer;

        /// <summary>Initializes a new instance of the <see cref="BufferedByteArrayStream"/> class.</summary>
        /// <param name="segments">The segments.</param>
        /// <exception cref="System.ArgumentException">The length of segments must be greater than zero.</exception>
        public BufferedByteArrayStream(IList<ArraySegment<Byte>> segments)
          : this(segments, null)
        {
        }

        internal BufferedByteArrayStream(IList<ArraySegment<Byte>> segments, ArrayPool<byte> bufferManager)
        {
            if (null == segments) { throw new ArgumentNullException(nameof(segments)); }
            if (segments.Count <= 0) { throw new ArgumentException("The length of segments must be greater than zero."); }

            m_segments = segments;
            m_currentSegmentOffset = segments[0].Offset;

            //m_length = segments.AsQueryExpr().Select(x => x.Count).Sum().Run();
            m_length = segments.Select(x => x.Count).Sum();

            m_currentSegment = segments[0];
            m_currentSegmentOffset = 0;
            m_currentSegmentPosition = 0;
            Position = 0;

            m_bufferManager = bufferManager;
        }

        #region -- IBufferedStreamCloneable Members --

        Stream IBufferedStreamCloneable.Clone()
        {
            var copy = new BufferedByteArrayStream(this.m_segments);
            copy.m_bufferManager = this.m_bufferManager;
            copy.m_callerReturnsBuffer = true;
            return copy;
        }

        #endregion

        #region -- IBufferedStream Members --

        public bool IsReadOnly => !CanWrite;

        int IBufferedStream.Length => (int)this.Length;

        public void CopyToSync(Stream destination)
        {
            if (null == destination) { throw new ArgumentNullException(nameof(destination)); }

            var count = (int)(m_length - m_position);

            while (count > 0)
            {
                var readAmount = Math.Min(m_currentSegment.Count - m_currentSegmentOffset, count);
                destination.Write(m_currentSegment.Array, m_currentSegment.Offset + m_currentSegmentOffset, readAmount);
                count -= readAmount;
                this.Position = m_position + readAmount;
            }
        }

        public void CopyToSync(Stream destination, int bufferSize) => CopyToSync(destination);

        public void CopyToSync(ArraySegment<Byte> destination)
        {
            var count = Math.Min((int)(m_length - m_position), destination.Count);
            var offset = destination.Offset;

            while (count > 0)
            {
                var readAmount = Math.Min(m_currentSegment.Count - m_currentSegmentOffset, count);

                System.Buffer.BlockCopy(m_currentSegment.Array, m_currentSegment.Offset + m_currentSegmentOffset, destination.Array, offset, readAmount);

                offset += readAmount;
                count -= readAmount;
                this.Position = m_position + readAmount;
            }
        }

        private async Task InternalCopyToAsync(Stream destination)
        {
            if (null == destination) { throw new ArgumentNullException(nameof(destination)); }

            var count = (int)(m_length - m_position);

            while (count > 0)
            {
                var readAmount = Math.Min(m_currentSegment.Count - m_currentSegmentOffset, count);
                await destination.WriteAsync(m_currentSegment.Array, m_currentSegment.Offset + m_currentSegmentOffset, readAmount);
                count -= readAmount;
                this.Position = m_position + readAmount;
            }
        }

#if NET40
        Task IBufferedStream.CopyToAsync(Stream destination)
        {
            return InternalCopyToAsync(destination);
        }

        Task IBufferedStream.CopyToAsync(Stream destination, int bufferSize)
        {
            return InternalCopyToAsync(destination);
        }

        Task IBufferedStream.CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) { return TaskConstants.Canceled; }
            return InternalCopyToAsync(destination);
        }

        public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) { return TaskConstants.Canceled; }
            return InternalCopyToAsync(destination);
        }
#else
#if NETSTANDARD2_0
        public Task CopyToAsync(Stream destination, CancellationToken cancellationToken) => CopyToAsync(destination, StreamToStreamCopy.DefaultBufferSize, cancellationToken);
#endif

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) { return TaskConstants.Canceled; }
            return InternalCopyToAsync(destination);
        }
#endif

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (c_on == Interlocked.CompareExchange(ref m_isDisposed, c_on, c_off)) { return; }

            if (m_callerReturnsBuffer) { return; }
            if (disposing)
            {
                if (m_bufferManager != null)
                {
                    foreach (var item in m_segments)
                    {
                        m_bufferManager.Return(item.Array);
                    }
                }
                // 直接赋空值，不能调用 clear 方法
                m_segments = null;
            }
        }

        // 继承类所用缓存池有可能不同，不再提供此方法
        //public virtual ArraySegmentWrapper<byte> ToArraySegment()
        //{
        //  if (m_segments == null || m_segments.Count <= 0 || m_length == 0) { return ArraySegmentWrapper<byte>.Empty; }

        //  if (m_segments.Count > 1)
        //  {
        //    var totalSize = (int)m_length;
        //    var buffer = m_bufferManager.TakeBuffer(totalSize);

        //    int offset = 0;
        //    for (int i = 0; i < m_segments.Count; i++)
        //    {
        //      var chunk = m_segments[i];
        //      Buffer.BlockCopy(chunk.Array, chunk.Offset, buffer, offset, chunk.Count);
        //      offset += chunk.Count;
        //    }

        //    return new ArraySegmentWrapper<byte>(buffer, 0, totalSize);
        //  }
        //  else
        //  {
        //    m_callerReturnsBuffer = true;
        //    var chunk = m_segments[0];
        //    return new ArraySegmentWrapper<byte>(chunk.Array, chunk.Offset, chunk.Count);
        //  }
        //}

        /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports reading.</summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override Boolean CanRead
        {
            get { return m_position < m_length; }
        }

        /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports seeking.</summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override Boolean CanSeek
        {
            get { return true; }
        }

        /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports writing.</summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override Boolean CanWrite
        {
            get { return false; }
        }

        /// <summary>When overridden in a derived class, gets the length in bytes of the stream.</summary>
        /// <returns>A Int64 value representing the length of the stream in bytes.</returns>
        public override Int64 Length
        {
            get { return m_length; }
        }

        /// <summary>When overridden in a derived class, gets or sets the position within the current stream.</summary>
        /// <returns>The current position within the stream.</returns>
        public override Int64 Position
        {
            get { return m_position; }
            set
            {
                if (value < 0L) { throw new ArgumentOutOfRangeException("Position must not be negative."); }
                if (value > m_length) { throw new ArgumentOutOfRangeException("Position is beyond end of buffer."); }

                if (value > m_position)
                {
                    for (Int32 i = m_currentSegmentIndex; i < m_segments.Count; i++)
                    {
                        if (value - m_currentSegmentPosition < m_segments[i].Count || (value - m_currentSegmentPosition == m_segments[i].Count && value == m_length))
                        {
                            m_currentSegmentIndex = i;
                            m_currentSegment = m_segments[i];
                            m_currentSegmentOffset = (Int32)(value - m_currentSegmentPosition);

                            break;
                        }
                        else
                        {
                            m_currentSegmentPosition += m_segments[i].Count;
                        }
                    }
                }
                else if (value < m_position)
                {
                    for (Int32 i = m_currentSegmentIndex; i >= 0; i--)
                    {
                        if (value - m_currentSegmentPosition >= 0)
                        {
                            m_currentSegmentIndex = i;
                            m_currentSegment = m_segments[i];
                            m_currentSegmentOffset = (Int32)(value - m_currentSegmentPosition);

                            break;
                        }
                        else
                        {
                            /*  I need to subtract the length of the PREVIOUS segment.								
                             *  This should never thrown an exception because the progress of the first segment should be 0, and
                             *  the value must be at least 0 too... 
                             */
                            m_currentSegmentPosition -= m_segments[i - 1].Count;
                        }
                    }
                }

                m_position = value;
            }
        }

        /// <summary>When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified Byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based Byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, 
        /// or zero (0) if the end of the stream has been reached.</returns>
        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }
            if (offset < 0 || offset > buffer.Length) { throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset cannot be negative or beyond the length of minibuf."); }
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count), count, "The count cannot be negative."); }
            if (offset + count > buffer.Length) { throw new ArgumentOutOfRangeException(nameof(count), count, "The count from the offset goes beyond the length of minibuf."); }

            count = min(count, m_length - m_position, buffer.Length - offset); //  This is exactly how much of the buffer will be read.
            var res = count;  //  Copied so 'red' can be modified.

            while (count > 0)
            {
                // As much as possible within the current segment.
                var readAmount = Math.Min(m_currentSegment.Count - m_currentSegmentOffset, count);

                System.Buffer.BlockCopy(m_currentSegment.Array, m_currentSegment.Offset + m_currentSegmentOffset, buffer, offset, readAmount);

                // Increase the offset within the mini-buffer.
                offset += readAmount;
                // Decrease the number of bytes left to read.
                count -= readAmount;
                // Advance the position. This should also change the current segment and position within that segment.
                this.Position = m_position + readAmount;
            }

            return res;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static Int32 min(Int32 a, Int64 b, Int32 c)
        {
            Int32 m = c < a ? c : a;
            if (b < m) { m = (Int32)b; }
            return m;
        }

#if !NET40
        /// <summary>ReadAsync</summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<Int32> ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
        {
            if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }
            if (offset < 0 || offset > buffer.Length) { throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset cannot be negative or beyond the length of minibuf."); }
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count), count, "The count cannot be negative."); }
            if (offset + count > buffer.Length) { throw new ArgumentOutOfRangeException(nameof(count), count, "The count from the offset goes beyond the length of minibuf."); }

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskConstants<Int32>.Canceled;
            }
            try
            {
                var readNum = Read(buffer, offset, count);
                return Task.FromResult(readNum);
            }
            //catch (OperationCanceledException oce)
            //{
            //	return Task.FromCancellation<VoidTaskResult>(oce);
            //}
            catch (Exception ex2)
            {
                //return AsyncUtils.CreateTaskFromException<VoidTaskResult>(ex2);
                //tcs.TrySetException(ex2);
                return AsyncUtils.FromException<Int32>(ex2);
            }
        }
#endif

        /// <summary>Begins an asynchronous read operation.</summary>
        /// <param name="buffer">The buffer to read the data into</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data read from the stream</param>
        /// <param name="count">The maximum number of bytes to read</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests</param>
        /// <returns>An IAsyncResult that represents the asynchronous read, which could still be pending</returns>
        public override IAsyncResult BeginRead(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state)
        {
            var readNum = Read(buffer, offset, count);
            return new CompletedAsyncResult<Int32>(readNum, callback, state);
        }

        /// <summary>Waits for the pending asynchronous read to complete</summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish</param>
        /// <returns>The number of bytes read from the stream, between zero (0) and the number of bytes you requested. 
        /// Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available</returns>
        public override Int32 EndRead(IAsyncResult asyncResult)
        {
            return CompletedAsyncResult<Int32>.End(asyncResult);
        }

        /// <summary>When overridden in a derived class, sets the position within the current stream.</summary>
        /// <param name="offset">A Byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="System.ArgumentException">Cannot support seek from the end.</exception>
        /// <exception cref="System.Exception">Exceed the stream's end</exception>
        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0) { throw new ArgumentOutOfRangeException("Offset points before the start of the buffer."); }
                    if (offset > m_length) { throw new ArgumentOutOfRangeException("Offset points beyond the end of the buffer."); }

                    return Position = offset;

                case SeekOrigin.End:
                    if (offset > 0) { throw new ArgumentOutOfRangeException("Offset points beyond the end of the buffer."); }
                    if (offset < -m_length) { throw new ArgumentOutOfRangeException("Offset points before the start of the buffer."); }

                    return Position = m_length + offset;

                case SeekOrigin.Current:
                    if (offset < -m_position) { throw new ArgumentOutOfRangeException("Offset points before the start of the buffer."); }
                    if (offset > m_length - m_position) { throw new ArgumentOutOfRangeException("Offset points beyond the end of the buffer."); }

                    return Position = m_position + offset;
            }

            throw new Exception("Exceed the stream's end");
        }

        /// <summary>When overridden in a derived class, sets the length of the current stream.</summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public override void SetLength(Int64 value)
        {
            if (value < 0L || value > m_length) { throw new ArgumentOutOfRangeException(nameof(value)); }
            m_length = value;
        }

        /// <summary>When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void Flush()
        {
            //throw new NotSupportedException();
        }

        /// <summary>When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based Byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            throw new NotSupportedException();
        }

#if !NET40
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
#endif
    }
}