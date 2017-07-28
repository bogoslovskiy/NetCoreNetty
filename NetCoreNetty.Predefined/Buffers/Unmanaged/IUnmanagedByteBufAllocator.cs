using System;

namespace NetCoreNetty.Buffers.Unmanaged
{
    public interface IUnmanagedByteBufAllocator : IByteBufAllocator
    {
        IUnmanagedByteBuf GetDefaultUnmanaged();

        IUnmanagedByteBuf GetUnmanaged(int size);

        IUnmanagedByteBuf WrapDefault(IntPtr dataPtr, int filledSize);

        IntPtr GetDefaultDataIntPtr(out int size);
    }
}