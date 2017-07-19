using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    // TODO: new() в Generic'ах реализован через рефлексию. Надо реорганизовать.
    public interface IChannelPipeline
    {
        IChannel Channel { get; }
        
        void AddLast<TChannelHandler>(string name)
            where TChannelHandler : IChannelHandler, new();

        void AddLast(string name, IChannelHandlerProvider channelHandlerProvider);

        void AddBefore<TChannelHandler>(string targetName, string name)
            where TChannelHandler : IChannelHandler, new();

        void AddBefore(string targetName, string name, IChannelHandlerProvider channelHandlerProvider);

        void Replace<TChannelHandler>(string targetName, string name)
            where TChannelHandler : IChannelHandler, new();

        void Replace(string targetName, string name, IChannelHandlerProvider channelHandlerProvider);

        void ChannelReadCallback(IChannel channel, ByteBuf byteBuf);
    }
}