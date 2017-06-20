using NetCoreNetty.Buffers;

namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    public class HttpMethodHeaderMatchStep : IHttpMatchStep
    {
        private IHttpMatchStep _skipToCrLfStep;

        public void Init(IHttpMatchStep skipToCrLfStep)
        {
            _skipToCrLfStep = skipToCrLfStep;
        }

        public void Clear()
        {
        }

        public void Match(ByteBuf byteBuf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            newStep = _skipToCrLfStep;
        }
    }
}