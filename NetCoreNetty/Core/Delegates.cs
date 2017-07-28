using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    public class Delegates
    {
        public delegate void ChannelReadCallback(IChannel channel, ByteBuf byteBuf);
    }
}