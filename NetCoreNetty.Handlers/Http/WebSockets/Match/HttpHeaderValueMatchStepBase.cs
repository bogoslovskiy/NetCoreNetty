using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    abstract public class HttpHeaderValueMatchStepBase : IHttpMatchStep
    {
        private readonly byte[] _headerValueLower;
        private readonly byte[] _headerValueUpper;
        private readonly int _headerValueLen;

        private int _index;
        private bool _firstByte;
        // ReSharper disable once InconsistentNaming
        private bool _lastByteIsCR;

        private bool _headerValueMatched;
        private bool _headerValueMatchedCurrent;
        private bool _skipToNextCommaAndWhitespace;

        protected HttpHeaderValueMatchStepBase(
            byte[] headerValueLower,
            byte[] headerValueUpper)
        {
            _headerValueLower = headerValueLower;
            _headerValueUpper = headerValueUpper;
            _headerValueLen = _headerValueLower.Length;
        }

        public void Clear()
        {
            _index = 0;
            _firstByte = false;
            _lastByteIsCR = false;
            _headerValueMatched = false;
            _headerValueMatchedCurrent = false;
            _skipToNextCommaAndWhitespace = false;
        }

        public void Match(ByteBuf byteBuf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            newStep = null;

            bool crlf = false;
            bool matched = false;
            bool notMatched = false;

            while (byteBuf.ReadableBytes() > 0)
            {
                if (!_firstByte)
                {
                    _firstByte = true;
                    _index = 0;
                    _headerValueMatched = true;
                    _headerValueMatchedCurrent = false;
                    _skipToNextCommaAndWhitespace = false;
                }

                byte nextByte = byteBuf.ReadByte();

                #region CRLF

                if (nextByte == HttpHeaderConstants.LF)
                {
                    if (_lastByteIsCR)
                    {
                        crlf = true;
                        _skipToNextCommaAndWhitespace = false;
                        break;
                    }

                    throw new Exception();
                }
                if (nextByte == HttpHeaderConstants.CR)
                {
                    _lastByteIsCR = true;
                    continue;
                }

                _lastByteIsCR = false;

                #endregion

                if (nextByte == HttpHeaderConstants.Comma || nextByte == HttpHeaderConstants.Whitespace)
                {
                    if (_headerValueMatchedCurrent)
                    {
                        matched = true;
                        _skipToNextCommaAndWhitespace = false;
                        break;
                    }

                    _firstByte = false;
                    continue;
                }

                if (!_skipToNextCommaAndWhitespace)
                {
                    _headerValueMatched &=
                        _headerValueLen > _index &&
                        (nextByte == _headerValueLower[_index] ||
                         nextByte == _headerValueUpper[_index]);
                    if (!_headerValueMatched)
                    {
                        _headerValueMatchedCurrent = false;
                        _skipToNextCommaAndWhitespace = true;
                    }
                    else
                    {
                        if (_index == _headerValueLen - 1)
                        {
                            _headerValueMatchedCurrent = true;
                        }
                    }
                }

                _index++;
            }

            matched |= crlf && _headerValueMatchedCurrent;

            if (!matched && !_headerValueMatched && !_skipToNextCommaAndWhitespace)
            {
                notMatched = true;
            }

            if (matched)
            {
                Matched(crlf, ref state, out newStep);
            }

            if (notMatched)
            {
                NotMatched(crlf, ref state, out newStep);
            }
        }

        abstract protected void Matched(bool crLf, ref HttpMatchState state, out IHttpMatchStep newStep);

        abstract protected void NotMatched(bool crLf, ref HttpMatchState state, out IHttpMatchStep newStep);
    }
}