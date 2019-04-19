#if !NET40

namespace CuteAnt.Buffers
{
    public sealed class ArrayMemoryPool : ArrayMemoryPool<byte>
    {
        new public static readonly ArrayMemoryPool Instance = new ArrayMemoryPool();

        private ArrayMemoryPool() : base(BufferManager.Shared) { }
    }
}

#endif