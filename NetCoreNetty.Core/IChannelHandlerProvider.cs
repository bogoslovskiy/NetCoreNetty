namespace NetCoreNetty.Core
{
    public interface IChannelHandlerProvider
    {
        IChannelHandler GetHandler();
    }
}