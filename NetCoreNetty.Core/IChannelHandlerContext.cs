using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    public interface IChannelHandlerContext
    {
        IChannel Channel { get; }

        IChannelHandler Handler { get; }

        IChannelPipeline Pipeline { get; }

        IByteBufAllocator ChannelByteBufAllocator { get; }

        string Name { get; }

        void Read(object message);

        void Write(object message);
    }
}