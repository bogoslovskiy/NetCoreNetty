using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    public class Delegates
    {
        public delegate void ChannelReadCallback(ChannelBase channel, ByteBuf byteBuf);
    }
}