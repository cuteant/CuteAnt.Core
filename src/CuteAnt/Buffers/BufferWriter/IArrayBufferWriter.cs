#if !NET40

namespace CuteAnt.Buffers
{
    using System;
    using System.Buffers;

    public interface IArrayBufferWriter<T> : IBufferWriter<T>, IDisposable
    {
        ref T Origin { get; }
        ArraySegment<T> WrittenBuffer { get; }
        ReadOnlyMemory<T> WrittenMemory { get; }
        ReadOnlySpan<T> WrittenSpan { get; }
        IOwnedBuffer<T> OwnedWrittenBuffer { get; }
        int WrittenCount { get; }
        int Capacity { get; }
        int FreeCapacity { get; }

        void Clear();
        ArraySegment<T> GetBuffer(int sizeHint = 0);

        T[] ToArray();
    }
}

#endif