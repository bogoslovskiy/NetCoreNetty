using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Codecs.WebSockets.DecoderStateMachine
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

            throw new NotSupportedException();

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

            if (state.Mask)
            {
                nextStep = _readMaskingKeyStep;
            }
            else
            {
                nextStep = _readPayloadDataStep;
            }
        }
    }
}