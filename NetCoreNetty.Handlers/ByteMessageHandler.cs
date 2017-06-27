using System;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;

namespace NetCoreNetty.Handlers
{
    abstract public class ByteMessageHandler : ChannelHandlerBase
    {
	    private ByteBuf _cumulatedByteBuf;
        
        public sealed override void Read(IChannelHandlerContext ctx, object message)
        {
	        if (message == null)
	        {
		        throw new ArgumentNullException(nameof(message));
	        }
	        
			ByteBuf byteBuf = message as ByteBuf;
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

	        // Читаем.
	        Read(ctx, byteBuf);

	        // Если буфер не освобожден и в нем есть данные для чтения, 
	        // буфер должен объединиться со следующим буфером.
	        if (!byteBuf.Released && byteBuf.ReadableBytes() > 0)
	        {
		        _cumulatedByteBuf = byteBuf;
	        }
        }

	    abstract protected void Read(IChannelHandlerContext ctx, ByteBuf byteBuf);
    }
}
