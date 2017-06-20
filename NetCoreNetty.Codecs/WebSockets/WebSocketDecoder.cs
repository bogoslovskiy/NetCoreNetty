using NetCoreNetty.Buffers;
using NetCoreNetty.Codecs.WebSockets.DecoderStateMachine;
using NetCoreNetty.Core;

namespace NetCoreNetty.Codecs.WebSockets
{
    // TODO: пулинг
    public class WebSocketDecoder : ByteToMessageDecoder<WebSocketFrame>
    {
        private readonly WebSocketDecoderStateMachine _decoderStateMachine = new WebSocketDecoderStateMachine();

        protected override WebSocketFrame DecodeOne(IChannelHandlerContext ctx, ByteBuf byteBuf)
        {
            WebSocketFrame frame;
            _decoderStateMachine.Read(byteBuf, out frame);

            return frame;
        }
    }
}