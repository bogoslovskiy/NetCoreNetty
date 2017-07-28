using NetCoreNetty.Buffers;

namespace NetCoreNetty.Codecs.WebSockets.DecoderStateMachine
{
    class WebSocketReadMaskingKeyStep : IWebSocketDecoderStep
    {
        private IWebSocketDecoderStep _readPayloadDataStep;

        public void Init(IWebSocketDecoderStep readPayloadDataStep)
        {
            _readPayloadDataStep = readPayloadDataStep;
        }

        public void Clear()
        {
        }

        public void Read(
            ByteBuf byteBuf,
            ref WebSocketReadState state,
            out WebSocketFrame frame,
            out IWebSocketDecoderStep nextStep)
        {
            frame = null;
            nextStep = null;

            if (byteBuf.ReadableBytes() < 4)
            {
                return;
            }

            state.MaskingKey[0] = byteBuf.ReadByte();
            state.MaskingKey[1] = byteBuf.ReadByte();
            state.MaskingKey[2] = byteBuf.ReadByte();
            state.MaskingKey[3] = byteBuf.ReadByte();

            nextStep = _readPayloadDataStep;
        }
    }
}