using System;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;

namespace NetCoreNetty.Handlers
{
	// TODO: здесь утечка
    abstract public class ByteMessageHandler : ChannelHandlerBase
    {
	    private ByteBuf _cumulatedByteBuf = null;
        
        public sealed override void Read(IChannelHandlerContext ctx, object message)
        {
			ByteBuf byteBuf = message as ByteBuf;
			if (byteBuf == null)
			{
				throw new InvalidOperationException();
			}

	        if (_cumulatedByteBuf != null)
	        {
		        _cumulatedByteBuf.Append(byteBuf);
		        byteBuf = _cumulatedByteBuf;
		        _cumulatedByteBuf = null;
	        }

	        Read(ctx, byteBuf);

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

	    abstract protected void Read(IChannelHandlerContext ctx, ByteBuf byteBuf);
    }
}
