using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    public interface IChannel
    {
        IByteBufAllocator ByteBufAllocator { get; }

        void StartRead();

        void StopRead();

        void Write(ByteBuf byteBuf);
    }
}