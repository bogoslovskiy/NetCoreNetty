using System;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;

namespace NetCoreNetty.Codecs
{
    abstract public class ByteToMessageDecoder<T> : ChannelHandlerBase
        where T : class
    {
        private ByteBuf _cumulatedByteBuf;

        public sealed override void Read(IChannelHandlerContext ctx, object input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            ByteBuf byteBuf = input as ByteBuf;
            if (byteBuf == null)
            {
                throw new ArgumentException("Message is not ByteBuf.");
            }

            // Объединяем буферы, если предыдущий буфер не был прочитан до конца.
            if (_cumulatedByteBuf != null)
            {
                _cumulatedByteBuf.Append(byteBuf);
                byteBuf = _cumulatedByteBuf;
                _cumulatedByteBuf = null;
            }

            // Пока декодер возвращает объект и в буфере есть данные для чтения, есть возможность декодировать следующий
            // объект.
            // Если же декодер не вернул объект или буфер опустошен, то декодирование можно прервать до поступления
            // следующей порции данных для обработки в новом буфере.
            T message;
            do
            {
                message = DecodeOne(ctx, byteBuf);
                if (message != null)
                {
                    ctx.Read(message);
                }
            }
            while (message != null && byteBuf.ReadableBytes() > 0);

            // Если буфер не освобожден и в нем есть данные для чтения, 
            // буфер должен объединиться со следующим буфером.
            if (!byteBuf.Released && byteBuf.ReadableBytes() > 0)
            {
                _cumulatedByteBuf = byteBuf;
            }
        }

        abstract protected T DecodeOne(IChannelHandlerContext ctx, ByteBuf byteBuf);
    }
}