using NetCoreNetty.Buffers;

namespace NetCoreNetty.Predefined.Codecs.WebSockets.DecoderStateMachine
{
//     0                   1                   2                   3
//     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
//    +-+-+-+-+-------+-+-------------+-------------------------------+
//    |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
//    |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
//    |N|V|V|V|       |S|             |   (if payload len==126/127)   |
//    | |1|2|3|       |K|             |                               |
//    +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
//    |     Extended payload length continued, if payload len == 127  |
//    + - - - - - - - - - - - - - - - +-------------------------------+
//    |                               |Masking-key, if MASK set to 1  |
//    +-------------------------------+-------------------------------+
//    | Masking-key (continued)       |          Payload Data         |
//    +-------------------------------- - - - - - - - - - - - - - - - +
//    :                     Payload Data continued ...                :
//    + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
//    |                     Payload Data continued ...                |
//    +---------------------------------------------------------------+

    class WebSocketReadHeaderStep : IWebSocketDecoderStep
    {
        private IWebSocketDecoderStep _readExtendedLenStep;
        private IWebSocketDecoderStep _readMaskingKeyStep;
        private IWebSocketDecoderStep _readPayloadDataStep;

        public void Init(
            IWebSocketDecoderStep readExtendedLenStep,
            IWebSocketDecoderStep readMaskingKeyStep,
            IWebSocketDecoderStep readPayloadDataStep)
        {
            _readExtendedLenStep = readExtendedLenStep;
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

            if (byteBuf.ReadableBytes() < 2)
            {
                return;
            }

            state.Clear();

            byte headerByte1 = byteBuf.ReadByte();
            byte headerByte2 = byteBuf.ReadByte();

            state.Fin = (headerByte1 & Predefined.Codecs.WebSockets.Utils.MaskFin) == Predefined.Codecs.WebSockets.Utils.MaskFin;
            state.OpCode = (byte) (headerByte1 & Predefined.Codecs.WebSockets.Utils.MaskOpCode);
            state.Mask = (headerByte2 & Predefined.Codecs.WebSockets.Utils.MaskMask) == Predefined.Codecs.WebSockets.Utils.MaskMask;
            state.PayloadLen = (byte) (headerByte2 & Predefined.Codecs.WebSockets.Utils.MaskPayloadLen);

            if (state.PayloadLen > 125)
            {
                nextStep = _readExtendedLenStep;
            }
            else if (state.Mask)
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