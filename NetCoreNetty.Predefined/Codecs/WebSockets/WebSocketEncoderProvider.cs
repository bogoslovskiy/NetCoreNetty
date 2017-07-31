using NetCoreNetty.Core;

namespace NetCoreNetty.Predefined.Codecs.WebSockets
{
    public class WebSocketEncoderProvider : IChannelHandlerProvider
    {
        private readonly int _frameMaxSize;

        public WebSocketEncoderProvider(int frameMaxSize)
        {
            _frameMaxSize = frameMaxSize;
        }
        
        public IChannelHandler GetHandler()
        {
            return new WebSocketEncoder(_frameMaxSize);
        }
    }
}