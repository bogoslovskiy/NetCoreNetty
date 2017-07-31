using NetCoreNetty.Core;

namespace NetCoreNetty.Predefined.Codecs.WebSockets
{
    public class WebSocketDecoderProvider : IChannelHandlerProvider
    {
        private readonly int _frameMaxSize;
        
        public WebSocketDecoderProvider(int frameMaxSize)
        {
            _frameMaxSize = frameMaxSize;
        }
        
        public IChannelHandler GetHandler()
        {
            return new WebSocketDecoder(_frameMaxSize);
        }
    }
}