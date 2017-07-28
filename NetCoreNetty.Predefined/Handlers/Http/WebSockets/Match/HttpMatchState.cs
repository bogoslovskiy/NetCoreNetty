using System.Diagnostics;

namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    [DebuggerDisplay(
        "Connection:{ConnectionHeaderMatched}, Upgrade:{UpgradeHeaderMatched}, SecWebSocketVersion:{SecWebSocketVersionHeaderMatched}, SecWebSocketKey:{SecWebSocketKeyHeaderMatched}"
    )]
    public struct HttpMatchState
    {
        public bool ConnectionHeaderMatched;
        public bool UpgradeHeaderMatched;
        public bool SecWebSocketVersionHeaderMatched;
        public bool SecWebSocketKeyHeaderMatched;

        public bool ConnectionHeaderValueMatched;
        public bool UpgradeHeaderValueMatched;
        public bool SecWebSocketVersionHeaderValueMatched;
        public bool SecWebSocketKeyHeaderValueMatched;

        public byte[] SecWebSocketKey;
        public int SecWebSocketKeyLen;

        public void Clear()
        {
            ConnectionHeaderMatched = false;
            UpgradeHeaderMatched = false;
            SecWebSocketVersionHeaderMatched = false;
            SecWebSocketKeyHeaderMatched = false;

            ConnectionHeaderValueMatched = false;
            UpgradeHeaderValueMatched = false;
            SecWebSocketVersionHeaderValueMatched = false;
            SecWebSocketKeyHeaderValueMatched = false;
        }
    }
}