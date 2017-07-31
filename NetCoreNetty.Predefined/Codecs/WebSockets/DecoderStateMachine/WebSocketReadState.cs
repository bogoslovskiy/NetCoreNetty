namespace NetCoreNetty.Predefined.Codecs.WebSockets.DecoderStateMachine
{
    struct WebSocketReadState
    {
        public bool Fin;
        public byte OpCode;
        public bool Mask;
        public byte PayloadLen;
        public ulong ExtendedPayloadLen;
        public byte[] MaskingKey;

        public WebSocketReadState(
            bool fin,
            byte opCode,
            bool mask,
            byte payloadLen,
            ulong extendedPayloadLen,
            byte[] maskingKey)
        {
            Fin = fin;
            OpCode = opCode;
            Mask = mask;
            PayloadLen = payloadLen;
            ExtendedPayloadLen = extendedPayloadLen;
            MaskingKey = maskingKey;
        }

        public void Clear()
        {
            Fin = false;
            OpCode = 0;
            Mask = false;
            PayloadLen = 0;
            ExtendedPayloadLen = 0;
            MaskingKey[0] = 0;
            MaskingKey[1] = 0;
            MaskingKey[2] = 0;
            MaskingKey[3] = 0;
        }

        public ulong GetPayloadLen()
        {
            return ExtendedPayloadLen > 0
                ? ExtendedPayloadLen
                : PayloadLen;
        }
    }
}