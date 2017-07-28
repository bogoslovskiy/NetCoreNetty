namespace NetCoreNetty.Core
{
    public interface IChannelHandler
    {
        void Read(IChannelHandlerContext ctx, object message);

        void Write(IChannelHandlerContext ctx, object message);
    }
}