using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Predefined.Buffers.Unmanaged
{
    // TODO: привести в порядок (избавиться от лишних методов в интерфейсе
    public interface IUnmanagedByteBufAllocator : IByteBufAllocator
    {
        IUnmanagedByteBuf GetDefaultUnmanaged();

        IUnmanagedByteBuf GetUnmanaged(int size);

        IUnmanagedByteBuf WrapDefault(IntPtr dataPtr, int filledSize);

        IntPtr GetDefaultDataIntPtr(out int size);
    }
}