using System;
using NetCoreNetty.Buffers;
using NetCoreNetty.Codecs;
using NetCoreNetty.Core;
using NetCoreNetty.Utils.ByteConverters;

namespace NetCoreNetty.Predefined.Codecs.WebSockets
{
    // TODO: посмотреть в спецификации, требуется ли маска для серверного ответа (по-моему нет)
    public class WebSocketEncoder : MessageToByteEncoder<WebSocketFrame>
    {
        private readonly int _frameMaxSize;
        
        public WebSocketEncoder(int frameMaxSize)
        {
            _frameMaxSize = frameMaxSize;
        }
        
        protected override void Reset()
        {
        }

        protected override ByteBuf Encode(IChannelHandlerContext ctx, WebSocketFrame message, out bool continueEncoding)
        {
            // TODO: временно
            continueEncoding = false;
            // TODO: пулинг + нормальная реализация

            // TODO: RemainBytes?
            int frameDataSize = message.Bytes.Length;

            // TODO: примерно!
            if (frameDataSize > 65536)
            {
                throw new NotImplementedException("Big message is not supported.");
            }

            // TODO: Mask
            bool mask = false;
            int maskingKey = 0;

            int len =
                frameDataSize +
                2 /* headerSize */ +
                (mask ? 4 : 0) +
                (frameDataSize <= 125
                    ? 0
                    : (frameDataSize == 65536
                        ? 2
                        : 8));

            // TODO: разбиение по буферам, буферы фиксированного размера.
            ByteBuf byteBuf = ctx.ChannelByteBufAllocator.GetDefault();

            byte opCode = Utils.GetFrameOpCode(message.Type);
            if (message.IsFinal)
            {
                opCode = (byte)(opCode | Utils.MaskFin);
            }

            byteBuf.Write(opCode);

            byte payloadLenAndMask;

            if (frameDataSize <= 125)
            {
                payloadLenAndMask = (byte) frameDataSize;
            }
            else if (frameDataSize <= 65536)
            {
                payloadLenAndMask = 126;
            }
            else
            {
                // TODO: сюда пока что попасть не можем - вверху есть проверка и исключение
                payloadLenAndMask = 127;
            }

            byte payloadLen = payloadLenAndMask;

            if (mask)
            {
                payloadLenAndMask = (byte)(payloadLenAndMask | Predefined.Codecs.WebSockets.Utils.MaskMask);
            }

            byteBuf.Write(payloadLenAndMask);

            if (payloadLen == 126)
            {
                ByteUnion2 byteUnion2 = new ByteUnion2();
                byteUnion2.UShort = (ushort)frameDataSize;
                byteBuf.Write(byteUnion2.B2);
                byteBuf.Write(byteUnion2.B1);
            }
            else
            {
                // TODO: сюда пока что попасть не можем - вверху есть проверка и исключение
            }

            if (mask)
            {
                ByteUnion4 byteUnion4 = new ByteUnion4();
                byteUnion4.Int = maskingKey;
                byteBuf.Write(byteUnion4.B4);
                byteBuf.Write(byteUnion4.B3);
                byteBuf.Write(byteUnion4.B2);
                byteBuf.Write(byteUnion4.B1);
            }

            // TODO: Маска + оптимизация
            for (int i = 0; i < frameDataSize; i++)
            {
                byteBuf.Write(message.Bytes[i]);
            }

            return byteBuf;
        }
    }
}