#if !NET40

namespace CuteAnt.Buffers
{
    using System.Buffers;

    public sealed class ArrayBufferWriter : ArrayBufferWriter<ArrayBufferWriter>
    {
        public ArrayBufferWriter() : base(BufferManager.Shared) { }

        public ArrayBufferWriter(int initialCapacity) : base(BufferManager.Shared, initialCapacity) { }

        public ArrayBufferWriter(ArrayPool<byte> arrayPool) : base(arrayPool) { }

        public ArrayBufferWriter(ArrayPool<byte> arrayPool, int initialCapacity) : base(arrayPool, initialCapacity) { }
    }
}

#endif