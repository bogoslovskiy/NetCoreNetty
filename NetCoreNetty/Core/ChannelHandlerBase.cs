namespace NetCoreNetty.Core
{
    abstract public class ChannelHandlerBase : IChannelHandler
    {
        public virtual void Read(IChannelHandlerContext ctx, object message)
        {
            ctx.Read(message);
        }

        public virtual void Write(IChannelHandlerContext ctx, object message)
        {
            ctx.Write(message);
        }
    }
}