using NetCoreNetty.Buffers;

namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    public interface IHttpMatchStep
    {
        void Clear();

        void Match(ByteBuf byteBuf, ref HttpMatchState state, out IHttpMatchStep newStep);
    }
}