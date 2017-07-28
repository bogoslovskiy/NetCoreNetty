using System;

namespace NetCoreNetty.Buffers.Unmanaged
{
    public interface IUnmanagedByteBuf
    {
        void GetReadable(out IntPtr dataPtr, out int length);

        void SetWrite(int write);
    }
}