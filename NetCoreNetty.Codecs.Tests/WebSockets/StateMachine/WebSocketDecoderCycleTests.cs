using System.Collections.Generic;
using System.Text;
using NetCoreNetty.Buffers;
using NetCoreNetty.Codecs.WebSockets;
using NetCoreNetty.Codecs.WebSockets.DecoderStateMachine;
using Xunit;

namespace NetCoreNetty.Codecs.Tests.WebSockets.StateMachine
{
    public class WebSocketDecoderCycleTests
    {
        static public IEnumerable<object[]> Test1Data => new[]
        {
            new object[]
            {
                new byte[]{0x81, 0x05, 0x48, 0x65, 0x6c, 0x6c, 0x6f}, true, false, 1, 5, "Hello"
            },
            new object[]
            {
                new byte[]{0x81, 0x05, 0x57, 0x6f, 0x72, 0x6c, 0x64}, true, false, 1, 5, "World"
            },
            new object[]
            {
                new byte[]{0x81, 0x0A, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x57, 0x6f, 0x72, 0x6c, 0x64}, true, false, 1, 10, "HelloWorld"
            }
        };

        [Theory, MemberData(nameof(Test1Data))]
        public void WebSocketDecoderCycleTest1(
            byte[] message,
            bool fin,
            bool mask,
            byte opCode,
            int payloadLen,
            string payloadTextData)
        {
            ByteBuf byteBuf = new SimpleByteBuf(message);

            var decoderCycle = new WebSocketDecoderStateMachine();

            WebSocketFrame frame;

            decoderCycle.Read(byteBuf, out frame);

            Assert.True(frame != null);
            Assert.True(frame.IsFinal = fin);
            Assert.True(frame.Type == WebSocketFrameType.Text);
            Assert.True(frame.Text.Equals(payloadTextData));
        }
    }
}