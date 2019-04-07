#if !NET40

namespace CuteAnt.Buffers
{
    using System;

    public interface IOwnedBuffer<T> : IDisposable
    {
        int Count { get; }

        ArraySegment<T> Buffer { get; }

        ReadOnlyMemory<T> Memory { get; }

        ReadOnlySpan<T> Span { get; }
    }
}

#endif