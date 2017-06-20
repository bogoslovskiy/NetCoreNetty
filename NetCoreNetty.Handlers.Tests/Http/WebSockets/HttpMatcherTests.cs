using System.Collections.Generic;
using System.Text;
using NetCoreNetty.Buffers;
using NetCoreNetty.Handlers.Http.WebSockets.Match;
using Xunit;

namespace NetCoreNetty.Handlers.Tests.Http.WebSockets
{
    public class HttpMatcherTests
    {
        static public IEnumerable<object[]> Test1Data => new[]
        {
            new object[]
            {
                new[] { "GET /chat HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 8\r\n" },
                new[] { false },
                false
            },
            new object[]
            {
                new[] { "GET /chat HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-Web" },
                new[] { true },
                false
            },
            new object[]
            {
                new[]
                {
                    "GET /chat HTTP/1.1\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n\r\n"
                },
                new[] { false },
                true
            },
        };

        [Theory, MemberData(nameof(Test1Data))]
        public void TestByteBufSkip(string[] asciiBuffers, bool[] continueMatch, bool httpMatched)
        {
            var httpMatcher = new HttpMatcher();
            httpMatcher.Clear();

            SimpleByteBuf byteBuf = null;

            for (int i = 0; i < asciiBuffers.Length; i++)
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(asciiBuffers[i]);

                if (byteBuf == null)
                {
                    byteBuf = new SimpleByteBuf(asciiBytes);
                }
                else
                {
                    byteBuf = byteBuf.Concat(asciiBytes);
                }

                bool continueMatching;
                httpMatcher.Match(byteBuf, out continueMatching);

                Assert.True(continueMatching == continueMatch[i]);
            }

            Assert.True(httpMatched == GetHttpMatched(httpMatcher.GetMatchingState));
        }

        private bool GetHttpMatched(HttpMatchState state)
        {
            return
                state.ConnectionHeaderMatched &&
                state.ConnectionHeaderValueMatched &&
                state.UpgradeHeaderMatched &&
                state.UpgradeHeaderValueMatched &&
                state.SecWebSocketVersionHeaderMatched &&
                state.SecWebSocketVersionHeaderValueMatched &&
                state.SecWebSocketKeyHeaderMatched &&
                state.SecWebSocketKeyHeaderValueMatched;
        }
    }
}