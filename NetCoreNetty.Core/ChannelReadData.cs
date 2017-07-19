using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    public struct ChannelReadData
    {
        public IChannelPipeline ChannelPipeline;

        public ByteBuf ByteBuffer;

        public ChannelReadData(IChannelPipeline channelPipeline, ByteBuf byteBuffer)
        {
            ChannelPipeline = channelPipeline;
            ByteBuffer = byteBuffer;
        }
    }
}