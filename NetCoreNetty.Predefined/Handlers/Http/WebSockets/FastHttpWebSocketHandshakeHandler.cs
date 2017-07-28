using System.Threading.Tasks;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;
using NetCoreNetty.Handlers.Http.WebSockets.Match;
using NetCoreNetty.Handlers.Http.WebSockets.Response;

namespace NetCoreNetty.Handlers.Http.WebSockets
{
    public class FastHttpWebSocketHandshakeHandler : ByteMessageHandler
    {
        private readonly string _webSocketsMiddlewareName;
        private readonly IChannelHandlerProvider _webSocket13DecoderProvider;
        private readonly IChannelHandlerProvider _webSocket13EncoderProvider;

        private HttpMatcher _httpMatcher;

        public FastHttpWebSocketHandshakeHandler(
            string webSocketsMiddlewareName,
            IChannelHandlerProvider webSocket13DecoderProvider,
            IChannelHandlerProvider webSocket13EncoderProvider)
        {
            _webSocketsMiddlewareName = webSocketsMiddlewareName;
            _webSocket13DecoderProvider = webSocket13DecoderProvider;
            _webSocket13EncoderProvider = webSocket13EncoderProvider;
        }

        protected override void Read(IChannelHandlerContext ctx, ByteBuf byteBuf)
        {
            if (byteBuf.ReadableBytes() > 0)
            {
                if (_httpMatcher == null)
                {
                    _httpMatcher = new HttpMatcher();
                }

                bool continueMatching;
                _httpMatcher.Match(byteBuf, out continueMatching);

                if (continueMatching)
                {
                    // Как минимум мы можем освободить прочитанную часть.
                    byteBuf.ReleaseReaded();
                    return;
                }

                // TODO: если будет реализован проброс, то буфер освобождать нельзя, а надо будет просто сделать возврат в начало чтения.
                // Освобождаем буфер, т.к. больше матчить не будем.
                byteBuf.Release();

                HttpMatchState state = _httpMatcher.GetMatchingState;

                bool handshake = HandshakeMatched(state);
                if (handshake)
                {
                    ByteBuf outByteBuf = SwitchingProtocolResponse.Get(ctx, state.SecWebSocketKey, state.SecWebSocketKeyLen);

                    _httpMatcher.Clear();

                    // TODO: !!!! временное решение !!!!
                    Task writeTask = Task.Factory.StartNew(
                        () =>
                        {
                            ctx.Write(outByteBuf);
                        }
                    );
                    Task transformationTask = writeTask.ContinueWith(
                        (writeTaskArg) =>
                        {
                            if (writeTask.IsCompleted)
                            {
                                ctx.Pipeline.AddBefore(
                                    ctx.Name,
                                    _webSocketsMiddlewareName + "Encoder",
                                    _webSocket13EncoderProvider
                                );
                                ctx.Pipeline.Replace(
                                    ctx.Name,
                                    _webSocketsMiddlewareName + "Decoder",
                                    _webSocket13DecoderProvider
                                );
                            }
                        }
                    );
                    transformationTask.Wait();
                }
                else
                {
                    // TODO: проброс или bad response
                    
                    _httpMatcher.Clear();
                }
            }
        }

        private bool HandshakeMatched(HttpMatchState state)
        {
            return
                state.ConnectionHeaderMatched &
                state.ConnectionHeaderValueMatched &
                state.UpgradeHeaderMatched &
                state.UpgradeHeaderValueMatched &
                state.SecWebSocketVersionHeaderMatched &
                state.SecWebSocketVersionHeaderValueMatched &
                state.SecWebSocketKeyHeaderMatched &
                state.SecWebSocketKeyHeaderValueMatched;
        }
    }
}