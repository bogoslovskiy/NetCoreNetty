using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Predefined.Handlers.Http.WebSockets.Match
{
    public class HttpHeaderNameMatchStep : IHttpMatchStep
    {
        static private readonly int ConnectionHeaderLen = HttpHeaderConstants.ConnectionLower.Length;
        static private readonly int UpgradeHeaderLen = HttpHeaderConstants.UpgradeLower.Length;
        static private readonly int SecWebSocketVersionHeaderLen = HttpHeaderConstants.SecWebsocketVersionLower.Length;
        static private readonly int SecWebSocketKeyHeaderLen = HttpHeaderConstants.SecWebsocketKeyLower.Length;

        private IHttpMatchStep _skipToCrLfMatchStep;
        private IHttpMatchStep _connectionHeaderValueMatchStep;
        private IHttpMatchStep _upgradeHeaderValueMatchStep;
        private IHttpMatchStep _secWebSocketVersionHeaderValueMatchStep;
        private IHttpMatchStep _secWebSocketKeyHeaderValueMatchStep;

        private int _index;
        private bool _firstByte;
        private bool _lastByteIsColon;
        // ReSharper disable once InconsistentNaming
        private bool _lastByteIsCR;

        private bool _connectionHeaderMatched;
        private bool _upgradeHeaderMatched;
        private bool _secWebSocketVersionHeaderMatched;
        private bool _secWebSocketKeyHeaderMatched;

        private bool _skipConnectionHeader;
        private bool _skipUpgradeHeader;
        private bool _skipSecWebSocketVersionHeader;
        private bool _skipSecWebSocketKeyHeader;

        public void Init(
            IHttpMatchStep skipToCrLfMatchStep,
            IHttpMatchStep connectionHeaderValueMatchStep,
            IHttpMatchStep upgradeHeaderValueMatchStep,
            IHttpMatchStep secWebSocketVersionHeaderValueMatchStep,
            IHttpMatchStep secWebSocketKeyHeaderValueMatchStep)
        {
            _skipToCrLfMatchStep = skipToCrLfMatchStep;
            _connectionHeaderValueMatchStep = connectionHeaderValueMatchStep;
            _upgradeHeaderValueMatchStep = upgradeHeaderValueMatchStep;
            _secWebSocketVersionHeaderValueMatchStep = secWebSocketVersionHeaderValueMatchStep;
            _secWebSocketKeyHeaderValueMatchStep = secWebSocketKeyHeaderValueMatchStep;
        }

        public void Clear()
        {
            _index = 0;
            _firstByte = false;
            _lastByteIsColon = false;
            _lastByteIsCR = false;

            _connectionHeaderMatched = false;
            _upgradeHeaderMatched = false;
            _secWebSocketVersionHeaderMatched = false;
            _secWebSocketKeyHeaderMatched = false;

            _skipConnectionHeader = false;
            _skipUpgradeHeader = false;
            _skipSecWebSocketVersionHeader = false;
            _skipSecWebSocketKeyHeader = false;
        }

        public void Match(ByteBuf byteBuf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            newStep = null;

            bool colonAndWhitespace = false;
            bool crlf = false;
            bool allNotMatched = false;

            while (byteBuf.ReadableBytes() > 0)
            {
                if (!_firstByte)
                {
                    _firstByte = true;

                    _connectionHeaderMatched = true;
                    _upgradeHeaderMatched = true;
                    _secWebSocketVersionHeaderMatched = true;
                    _secWebSocketKeyHeaderMatched = true;
                }

                byte nextByte = byteBuf.ReadByte();

                #region ": "

                if (nextByte == HttpHeaderConstants.Whitespace)
                {
                    if (_lastByteIsColon)
                    {
                        colonAndWhitespace = true;
                        break;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                if (nextByte == HttpHeaderConstants.Colon)
                {
                    _lastByteIsColon = true;
                    continue;
                }
                else
                {
                    _lastByteIsColon = false;
                    colonAndWhitespace = false;
                }

                #endregion

                #region CRLF

                if (nextByte == HttpHeaderConstants.LF)
                {
                    if (_lastByteIsCR)
                    {
                        crlf = true;
                        break;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                if (nextByte == HttpHeaderConstants.CR)
                {
                    _lastByteIsCR = true;
                    continue;
                }
                else
                {
                    _lastByteIsCR = false;
                    crlf = false;
                }

                #endregion

                #region Headers matching

                if (!_skipConnectionHeader)
                {
                    _connectionHeaderMatched &=
                        ConnectionHeaderLen > _index &&
                        (nextByte == HttpHeaderConstants.ConnectionLower[_index] ||
                         nextByte == HttpHeaderConstants.ConnectionUpper[_index]);
                    if (!_connectionHeaderMatched)
                    {
                        _skipConnectionHeader = true;
                    }
                }

                if (!_skipUpgradeHeader)
                {
                    _upgradeHeaderMatched &=
                        UpgradeHeaderLen > _index &&
                        (nextByte == HttpHeaderConstants.UpgradeLower[_index] ||
                         nextByte == HttpHeaderConstants.UpgradeUpper[_index]);
                    if (!_upgradeHeaderMatched)
                    {
                        _skipUpgradeHeader = true;
                    }
                }

                if (!_skipSecWebSocketVersionHeader)
                {
                    _secWebSocketVersionHeaderMatched &=
                        SecWebSocketVersionHeaderLen > _index &&
                        (nextByte == HttpHeaderConstants.SecWebsocketVersionLower[_index] ||
                         nextByte == HttpHeaderConstants.SecWebsocketVersionUpper[_index]);
                    if (!_secWebSocketVersionHeaderMatched)
                    {
                        _skipSecWebSocketVersionHeader = true;
                    }
                }

                if (!_skipSecWebSocketKeyHeader)
                {
                    _secWebSocketKeyHeaderMatched &=
                        SecWebSocketKeyHeaderLen > _index &&
                        (nextByte == HttpHeaderConstants.SecWebsocketKeyLower[_index] ||
                         nextByte == HttpHeaderConstants.SecWebsocketKeyUpper[_index]);
                    if (!_secWebSocketKeyHeaderMatched)
                    {
                        _skipSecWebSocketKeyHeader = true;
                    }
                }

                #endregion

                allNotMatched =
                    !_connectionHeaderMatched &&
                    !_upgradeHeaderMatched &&
                    !_secWebSocketVersionHeaderMatched &&
                    !_secWebSocketKeyHeaderMatched;
                if (allNotMatched)
                {
                    break;
                }

                _index++;
            }

            if (crlf)
            {
                throw new Exception();
            }

            if (allNotMatched)
            {
                newStep = _skipToCrLfMatchStep;
                return;
            }

            if (colonAndWhitespace)
            {
                if (_connectionHeaderMatched)
                {
                    state.ConnectionHeaderMatched = true;
                    newStep = _connectionHeaderValueMatchStep;
                }
                else if (_upgradeHeaderMatched)
                {
                    state.UpgradeHeaderMatched = true;
                    newStep = _upgradeHeaderValueMatchStep;
                }
                else if (_secWebSocketVersionHeaderMatched)
                {
                    state.SecWebSocketVersionHeaderMatched = true;
                    newStep = _secWebSocketVersionHeaderValueMatchStep;
                }
                else if (_secWebSocketKeyHeaderMatched)
                {
                    state.SecWebSocketKeyHeaderMatched = true;
                    newStep = _secWebSocketKeyHeaderValueMatchStep;
                }
                else
                {
                    newStep = _skipToCrLfMatchStep;
                }
            }
        }
    }
}