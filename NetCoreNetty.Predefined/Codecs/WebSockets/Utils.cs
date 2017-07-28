using System;

namespace NetCoreNetty.Predefined.Codecs.WebSockets
{
    static public class Utils
    {
//         0                   1                   2                   3
//         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
//        +-+-+-+-+-------+-+-------------+-------------------------------+
//        |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
//        |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
//        |N|V|V|V|       |S|             |   (if payload len==126/127)   |
//        | |1|2|3|       |K|             |                               |
//        +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
//        |     Extended payload length continued, if payload len == 127  |
//        + - - - - - - - - - - - - - - - +-------------------------------+
//        |                               |Masking-key, if MASK set to 1  |
//        +-------------------------------+-------------------------------+
//        | Masking-key (continued)       |          Payload Data         |
//        +-------------------------------- - - - - - - - - - - - - - - - +
//        :                     Payload Data continued ...                :
//        + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
//        |                     Payload Data continued ...                |
//        +---------------------------------------------------------------+

        public const byte MaskFin = 1 << 7;
        public const byte MaskRsv1 = 1 << 6;
        public const byte MaskRsv2 = 1 << 5;
        public const byte MaskRsv3 = 1 << 4;
        public const byte MaskOpCode = 15;
        public const byte MaskMask = 128;
        public const byte MaskPayloadLen = 127;

        public const int MandatoryHeaderSizeInBytes = 2;
        public const int MaskingKeySizeInBytes = 4;
        public const int PayloadExtendedLen16SizeInBytes = 2;

        static public WebSocketFrameType GetFrameType(byte opCode)
        {
            switch (opCode)
            {
                case 0x1: return WebSocketFrameType.Text;
                case 0x2: return WebSocketFrameType.Binary;
                case 0x8: return WebSocketFrameType.Close;
                case 0x9: return WebSocketFrameType.Ping;
                case 0xA: return WebSocketFrameType.Pong;
            }

            throw new ArgumentOutOfRangeException(nameof(opCode), opCode, null);
        }

        static public byte GetFrameOpCode(WebSocketFrameType frameType)
        {
            switch (frameType)
            {
                case WebSocketFrameType.Text: return 0x1;
                case WebSocketFrameType.Binary: return 0x2;
                case WebSocketFrameType.Close: return 0x8;
                case WebSocketFrameType.Ping: return 0x9;
                case WebSocketFrameType.Pong: return 0xA;
            }

            throw new ArgumentOutOfRangeException(nameof(frameType), frameType, null);
        }
    }
}