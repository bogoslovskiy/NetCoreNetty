using NetCoreNetty.Buffers;
using NetCoreNetty.Codecs;
using NetCoreNetty.Core;
using NetCoreNetty.Predefined.Codecs.WebSockets.DecoderStateMachine;

namespace NetCoreNetty.Predefined.Codecs.WebSockets
{
    // TODO: пулинг декодера
    public class WebSocketDecoder : ByteToMessageDecoder<WebSocketFrame>
    {
        // Храним ссылку на буфер на время жизни декодера. Как только декодер будет передан в пул 
        // (если есть пуллинг декодеров) или будет финализирован сборщиком, буфер надо отдать в пул.
        private ByteBuf _byteBuf;
        
        ~WebSocketDecoder()
        {
            // Если клиент отключится от канала, то декодер будет финализирован (пока нет пуллинга).
            // Буфер чтения данных при этом можно аккуратно освободить (вернуть в пул).
            _byteBuf.Release();
        }
        
        private readonly WebSocketDecoderStateMachine _decoderStateMachine = new WebSocketDecoderStateMachine();

        protected override WebSocketFrame DecodeOne(IChannelHandlerContext ctx, ByteBuf byteBuf)
        {
            // Сохраняем ссылку на буфер, чтобы иметь возможность полностью освободить его, при деконструкции декодера.
            _byteBuf = byteBuf;
            
            WebSocketFrame frame;
            _decoderStateMachine.Read(byteBuf, out frame);

            // Как минимум мы можем освободить прочитанную часть.
            // Буфер при этом не освободится полностью.
            byteBuf.ReleaseReaded();
            
            return frame;
        }
    }
}