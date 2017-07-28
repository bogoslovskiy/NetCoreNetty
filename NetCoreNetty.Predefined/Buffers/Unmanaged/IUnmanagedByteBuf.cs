﻿using System;

namespace NetCoreNetty.Predefined.Buffers.Unmanaged
{
    public interface IUnmanagedByteBuf
    {
        void GetReadable(out IntPtr dataPtr, out int length);

        void SetWrite(int write);
    }
}