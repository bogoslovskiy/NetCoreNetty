using System;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;

namespace NetCoreNetty.Codecs
{
    abstract public class MessageToByteEncoder<T> : ChannelHandlerBase
        where T : class
    {
        public sealed override void Write(IChannelHandlerContext ctx, object input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            T message = input as T;
            if (message == null)
            {
                throw new InvalidOperationException();
            }

            bool continueEncoding;
            do
            {
                ByteBuf byteBuf = Encode(ctx, message, out continueEncoding);
                ctx.Write(byteBuf);
            }
            while (continueEncoding);
        }

        abstract protected ByteBuf Encode(IChannelHandlerContext ctx, T message, out bool continueEncoding);
    }
}