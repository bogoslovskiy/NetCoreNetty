namespace NetCoreNetty.Core
{
    public class DefaultChannelHandlerProvider<TChannelHandler> : IChannelHandlerProvider
        where TChannelHandler : IChannelHandler, new()
    {
        public IChannelHandler GetHandler()
        {
            return new TChannelHandler();
        }
    }
}