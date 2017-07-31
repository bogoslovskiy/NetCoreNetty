using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    // TODO: logger
    abstract public class ChannelBase
    {
        protected readonly IInboundBuffer InboundBuffer;

        public IByteBufAllocator ByteBufAllocator { get; }
        
        public IChannelPipeline ChannelPipeline { get; set; }

        protected ChannelBase(
            IByteBufAllocator channelByteBufAllocator,
            IInboundBuffer inboundBuffer)
        {
            ByteBufAllocator = channelByteBufAllocator;
            InboundBuffer = inboundBuffer;
        }
        
        abstract public void StartRead();

        abstract public void StopRead();

        abstract public void Write(ByteBuf byteBuf);

        protected void OnRead(ByteBuf byteBuf)
        {
            Console.WriteLine("ReadCallback. {0} bytes to read.", byteBuf.ReadableBytes());
            
            IChannelPipeline pipeline = this.ChannelPipeline;
            
            var channelReadData = new ChannelReadData(pipeline, byteBuf);

            InboundBuffer.Write(pipeline.InternalInboundBuffer, channelReadData);
        }
    }
}