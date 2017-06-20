using NetCoreNetty.Buffers;

namespace NetCoreNetty.Codecs.WebSockets.DecoderStateMachine
{
    // TODO: FIN + _headerStep.
    class WebSocketReadPayloadDataStep : IWebSocketDecoderStep
    {
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

            if (byteBuf.ReadableBytes() < state.PayloadLen)
            {
                return;
            }

            // TODO: alloc ByteBuf
            byte[] frameBytes = new byte[state.PayloadLen];

            // TODO: оптимизация
            for (int i = 0; i < state.PayloadLen; i++)
            {
                frameBytes[i] = byteBuf.ReadByte();
            }

            if (state.Mask)
            {
                for (int i = 0; i < frameBytes.Length; i++)
                {
                    frameBytes[i] ^= state.MaskingKey[i % 4];
                }
            }

            frame = new WebSocketFrame();
            frame.IsFinal = state.Fin;
            frame.Type = Utils.GetFrameType(state.OpCode);
            frame.Bytes = frameBytes;
        }
    }
}