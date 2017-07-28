using System.Text;

namespace NetCoreNetty.Predefined.Handlers.Http.WebSockets
{
    static public class HttpHeaderConstants
    {
        // ReSharper disable once InconsistentNaming
        static public readonly byte CR = (byte) '\r';
        // ReSharper disable once InconsistentNaming
        static public readonly byte LF = (byte) '\n';

        // TODO: заменить на конкретные ASCII байт коды.
        static public readonly byte Colon = Encoding.ASCII.GetBytes(":")[0];
        static public readonly byte Whitespace = Encoding.ASCII.GetBytes(" ")[0];
        static public readonly byte Comma = Encoding.ASCII.GetBytes(",")[0];

        static public readonly byte[] WebSockets13Token =
            Encoding.ASCII.GetBytes("258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
        public const int WebSockets13TokenLength = 36;

        static public readonly byte[] ConnectionLower = Encoding.ASCII.GetBytes("connection");
        static public readonly byte[] ConnectionUpper = Encoding.ASCII.GetBytes("CONNECTION");
        static public readonly byte[] UpgradeLower = Encoding.ASCII.GetBytes("upgrade");
        static public readonly byte[] UpgradeUpper = Encoding.ASCII.GetBytes("UPGRADE");
        static public readonly byte[] WebsocketLower = Encoding.ASCII.GetBytes("websocket");
        static public readonly byte[] WebsocketUpper = Encoding.ASCII.GetBytes("WEBSOCKET");
        static public readonly byte[] SecWebsocketVersionLower = Encoding.ASCII.GetBytes("sec-websocket-version");
        static public readonly byte[] SecWebsocketVersionUpper = Encoding.ASCII.GetBytes("SEC-WEBSOCKET-VERSION");
        static public readonly byte[] SecWebsocketKeyLower = Encoding.ASCII.GetBytes("sec-websocket-key");
        static public readonly byte[] SecWebsocketKeyUpper = Encoding.ASCII.GetBytes("SEC-WEBSOCKET-KEY");
        static public readonly byte[] Version13 = Encoding.ASCII.GetBytes("13");

        static public readonly byte[] SwitchingProtocolsHttpHeadersPart = Encoding.ASCII.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Accept: "
        );
    }
}