using NetCoreNetty.Buffers;

namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    public class HttpSecWebSocketKeyHeaderValueMatchStep : IHttpMatchStep
    {
        private IHttpMatchStep _crLfStep;

        public void Init(IHttpMatchStep crLfStep)
        {
            _crLfStep = crLfStep;
        }

        public void Clear()
        {
        }

        public void Match(ByteBuf byteBuf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            newStep = null;

            int read = byteBuf.ReadToOrRollback(
                HttpHeaderConstants.CR,
                HttpHeaderConstants.LF,
                state.SecWebSocketKey /* output */,
                0 /* startIndex */,
                state.SecWebSocketKey.Length /* len */
            );

            if (read < 0)
            {
                return;
            }

            // Если буфер смог дочитать до CRLF, значит в нем точно есть еще как минимум 2 байта CRLF.
            // Читаем их, чтобы сдвинуть.
            byteBuf.ReadByte();
            byteBuf.ReadByte();

            state.SecWebSocketKeyHeaderValueMatched = true;
            state.SecWebSocketKeyLen = read;

            newStep = _crLfStep;
        }
    }
}