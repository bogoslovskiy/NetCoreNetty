using System;
using NetCoreNetty.Concurrency;

namespace NetCoreNetty.Core
{
    public interface IInboundBuffer
    {
        void Write(CircularBuffer<ChannelReadData> buffer, ChannelReadData data);

        void StartRead(Action<ChannelReadData> callback);
    }
}