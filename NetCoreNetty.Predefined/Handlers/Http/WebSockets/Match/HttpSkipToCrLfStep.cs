using NetCoreNetty.Buffers;

namespace NetCoreNetty.Predefined.Handlers.Http.WebSockets.Match
{
    public class HttpSkipToCrLfStep : IHttpMatchStep
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

            int skipped = byteBuf.SkipTo(
                HttpHeaderConstants.CR /* stopByte1 */,
                HttpHeaderConstants.LF /* stopByte2 */,
                true /* include */
            );

            // Если пропустить не удается (результат -1, т.е. буфер прерывается до окончания чтения),
            // то остаемся в этом же состоянии. Может при следующем входе удастся продолжить.
            if (skipped < 0)
            {
                return;
            }

            newStep = _crLfStep;
        }
    }
}