using System;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;

namespace NetCoreNetty.Codecs
{
    abstract public class MessageToByteEncoder<T> : ChannelHandlerBase
        where T : class
    {
        public sealed override void Write(IChannelHandlerContext ctx, object message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            T messageObj = message as T;
            if (message == null)
            {
                throw new ArgumentException($"Message is not {typeof(T)}.");
            }

            // Пока объект кодируется в буфер, продолжаем отправлять буферы дальше по конвейеру.
            bool continueEncoding;
            do
            {
                ByteBuf byteBuf = Encode(ctx, messageObj, out continueEncoding);
                ctx.Write(byteBuf);
            }
            while (continueEncoding);
        }

        abstract protected ByteBuf Encode(IChannelHandlerContext ctx, T message, out bool continueEncoding);
    }
}