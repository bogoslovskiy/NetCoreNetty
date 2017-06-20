using NetCoreNetty.Core;

namespace NetCoreNetty.Handlers.Http.WebSockets
{
    public class FastHttpWebSocketHandshakeHandlerProvider : IChannelHandlerProvider
    {
        private readonly string _webSocketsMiddlewareName;
        private readonly IChannelHandlerProvider _webSocket13DecoderProvider;
        private readonly IChannelHandlerProvider _webSocket13EncoderProvider;

        public FastHttpWebSocketHandshakeHandlerProvider(
            string webSocketsMiddlewareName,
            IChannelHandlerProvider webSocket13DecoderProvider,
            IChannelHandlerProvider webSocket13EncoderProvider)
        {
            _webSocketsMiddlewareName = webSocketsMiddlewareName;
            _webSocket13DecoderProvider = webSocket13DecoderProvider;
            _webSocket13EncoderProvider = webSocket13EncoderProvider;
        }

        public IChannelHandler GetHandler()
        {
            return new FastHttpWebSocketHandshakeHandler(
                _webSocketsMiddlewareName,
                _webSocket13DecoderProvider,
                _webSocket13EncoderProvider
            );
        }
    }
}