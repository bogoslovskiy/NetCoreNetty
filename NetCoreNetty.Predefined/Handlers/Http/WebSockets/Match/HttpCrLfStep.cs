using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    public class HttpCrLfStep : IHttpMatchStep
    {
        private IHttpMatchStep _finishStep;
        private IHttpMatchStep _httpHeaderNameMatchStep;

        public void Init(IHttpMatchStep finishStep, IHttpMatchStep httpHeaderNameMatchStep)
        {
            _finishStep = finishStep;
            _httpHeaderNameMatchStep = httpHeaderNameMatchStep;
        }

        public void Clear()
        {
        }

        public void Match(ByteBuf byteBuf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            newStep = null;

            // Далее как минимум должны следовать либо 2 байта CRLF, либо следующий заголовок со значением,
            // где тоже должно быть гораздо больше байт, с учетом того,
            // что название и значение разделено 2 байтами ": ".
            if (byteBuf.ReadableBytes() < 2)
            {
                return;
            }

            // Читаем следующие 2 байта.
            byte nextByte1 = byteBuf.ReadByte();
            byte nextByte2 = byteBuf.ReadByte();

            // Если CRLF - значит это второй CRLF, который по стандарту означает начало тела HTTP.
            // Но в нашем случае, тело нас не интересует, мы прочитали заголовки.
            if (nextByte1 == HttpHeaderConstants.CR && nextByte2 == HttpHeaderConstants.LF)
            {
                newStep = _finishStep;
                return;
            }

            // TODO: нормальные исключения
            if (nextByte1 == HttpHeaderConstants.CR && nextByte2 != HttpHeaderConstants.LF)
            {
                throw new Exception();
            }

            if (nextByte1 != HttpHeaderConstants.CR && nextByte2 == HttpHeaderConstants.LF)
            {
                throw new Exception();
            }

            // Если далее идут не CRLF, значит идет следующий заголовок.
            // Возвращаем чтение буфера на 2 байта обратно.
            byteBuf.Back(2);

            newStep = _httpHeaderNameMatchStep;
        }
    }
}