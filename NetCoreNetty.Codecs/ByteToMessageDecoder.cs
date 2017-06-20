using System;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;

namespace NetCoreNetty.Codecs
{
    // TODO: здесь утечка. буфер не освобождается и не освобождается его IntPtr ресурс.
    abstract public class ByteToMessageDecoder<T> : ChannelHandlerBase
        where T : class
    {
        private ByteBuf _cumulatedByteBuf = null;

        public sealed override void Read(IChannelHandlerContext ctx, object input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            ByteBuf byteBuf = input as ByteBuf;
            if (byteBuf == null)
            {
                throw new InvalidOperationException();
            }

            // Если ранее буфер не был освобожден, значит вновь пришедший буфер нужно склеить с предыдущим.

            T message = null;

            if (_cumulatedByteBuf != null)
            {
                _cumulatedByteBuf.Append(byteBuf);
                byteBuf = _cumulatedByteBuf;
                _cumulatedByteBuf = null;
            }

            do
            {
                message = DecodeOne(ctx, byteBuf);

                if (message != null)
                {
                    ctx.Read(message);
                }
            }
            while (message != null && byteBuf.ReadableBytes() > 0);

            // TODO: проверять, что буфер не Released
            if (byteBuf.ReadableBytes() > 0)
            {
                _cumulatedByteBuf = byteBuf;
            }

            // За освобождение буфера должна отвечать логика метода Decode.
            // Например, освобождать прочитанное. Тогда, если буфер считан полностью, то он освободится в пул,
            // иначе освободится (если это возможно) прочитанная часть буфера, а оставшаяся пойдет в аккумуляцию
            // с вновь приходящими буферами.
        }

        abstract protected T DecodeOne(IChannelHandlerContext ctx, ByteBuf byteBuf);
    }
}