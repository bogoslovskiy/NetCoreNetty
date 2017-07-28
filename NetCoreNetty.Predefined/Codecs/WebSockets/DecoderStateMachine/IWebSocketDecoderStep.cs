using NetCoreNetty.Buffers;

namespace NetCoreNetty.Codecs.WebSockets.DecoderStateMachine
{
    interface IWebSocketDecoderStep
    {
        void Clear();

        void Read(
            ByteBuf byteBuf,
            ref WebSocketReadState state,
            out WebSocketFrame frame,
            out IWebSocketDecoderStep nextStep);
    }
}