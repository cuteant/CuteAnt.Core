#if !NET40

namespace CuteAnt.Buffers
{
    using System.Buffers;

    public sealed class ThreadLocalBufferWriter : ThreadLocalBufferWriter<ThreadLocalBufferWriter>
    {
        public ThreadLocalBufferWriter() : base(BufferManager.Shared) { }

        public ThreadLocalBufferWriter(int initialCapacity) : base(BufferManager.Shared, initialCapacity) { }

        public ThreadLocalBufferWriter(ArrayPool<byte> arrayPool) : base(arrayPool) { }

        public ThreadLocalBufferWriter(ArrayPool<byte> arrayPool, int initialCapacity) : base(arrayPool, initialCapacity) { }
    }
}

#endif