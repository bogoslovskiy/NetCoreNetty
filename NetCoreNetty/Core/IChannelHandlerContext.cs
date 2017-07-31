using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    public interface IChannelHandlerContext
    {
        ChannelBase Channel { get; }

        IChannelHandler Handler { get; }

        IChannelPipeline Pipeline { get; }

        IByteBufAllocator ChannelByteBufAllocator { get; }

        string Name { get; }

        void Read(object message);

        void Write(object message);
    }
}