using NetCoreNetty.Buffers;

namespace NetCoreNetty.Predefined.Codecs.WebSockets.DecoderStateMachine
{
    class WebSocketReadExtendedPayloadLengthStep : IWebSocketDecoderStep
    {
        private IWebSocketDecoderStep _readMaskingKeyStep;
        private IWebSocketDecoderStep _readPayloadDataStep;

        public void Init(IWebSocketDecoderStep readMaskingKeyStep, IWebSocketDecoderStep readPayloadDataStep)
        {
            _readMaskingKeyStep = readMaskingKeyStep;
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

            if (state.PayloadLen == 126)
            {
                if (byteBuf.ReadableBytes() < 2)
                {
                    return;
                }

                state.ExtendedPayloadLen = byteBuf.ReadUShort();
            }
            else if (state.PayloadLen == 127)
            {
                if (byteBuf.ReadableBytes() < 8)
                {
                    return;
                }

                state.ExtendedPayloadLen = byteBuf.ReadULong();
            }

            nextStep = state.Mask 
                ? _readMaskingKeyStep 
                : _readPayloadDataStep;
        }
    }
}