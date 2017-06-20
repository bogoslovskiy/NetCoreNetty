﻿namespace NetCoreNetty.Buffers
{
    public interface IByteBufAllocator
    {
        ByteBuf GetDefault();

        ByteBuf Get(int size);

        void Release(ByteBuf buf);
    }
}