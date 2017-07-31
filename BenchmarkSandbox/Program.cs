using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using NetCoreNetty.Buffers;
using NetCoreNetty.Predefined.Codecs.WebSockets;
using NetCoreNetty.Predefined.Codecs.WebSockets.DecoderStateMachine;
using NetCoreNetty.Predefined.Handlers.Http.WebSockets.Match;

namespace BenchmarkSandbox
{
    [CoreJob]
    public class WebSocketDecoderBench
    {
        private byte[] _message;
        private ByteBuf _messageByteBuf1;
        private ByteBuf _messageByteBuf2;
        private ByteBuf _messageByteBuf3;
        private ByteBuf _messageByteBuf4;
        private ByteBuf _messageByteBuf5;
        private ByteBuf _messageByteBuf6;
        private ByteBuf _messageByteBuf7;
        private ByteBuf _messageByteBuf8;
        private ByteBuf _messageByteBuf9;
        private ByteBuf _messageByteBuf10;
        private WebSocketDecoderStateMachine _decoderStateMachine;

        public WebSocketDecoderBench()
        {
            _message = new byte[] {0x81, 0x0A, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x57, 0x6f, 0x72, 0x6c, 0x64};
            _messageByteBuf1 = new SimpleByteBuf(_message);
            _messageByteBuf2 = new SimpleByteBuf(_message);
            _messageByteBuf3 = new SimpleByteBuf(_message);
            _messageByteBuf4 = new SimpleByteBuf(_message);
            _messageByteBuf5 = new SimpleByteBuf(_message);
            _messageByteBuf6 = new SimpleByteBuf(_message);
            _messageByteBuf7 = new SimpleByteBuf(_message);
            _messageByteBuf8 = new SimpleByteBuf(_message);
            _messageByteBuf9 = new SimpleByteBuf(_message);
            _messageByteBuf10 = new SimpleByteBuf(_message);

            _decoderStateMachine = new WebSocketDecoderStateMachine(32);
        }

        [Benchmark]
        public void DecodeFrameSimpleBuf()
        {
            WebSocketFrame frame;

            _decoderStateMachine.Read(_messageByteBuf1, out frame);
            _decoderStateMachine.Read(_messageByteBuf2, out frame);
            _decoderStateMachine.Read(_messageByteBuf3, out frame);
            _decoderStateMachine.Read(_messageByteBuf4, out frame);
            _decoderStateMachine.Read(_messageByteBuf5, out frame);
            _decoderStateMachine.Read(_messageByteBuf6, out frame);
            _decoderStateMachine.Read(_messageByteBuf7, out frame);
            _decoderStateMachine.Read(_messageByteBuf8, out frame);
            _decoderStateMachine.Read(_messageByteBuf9, out frame);
            _decoderStateMachine.Read(_messageByteBuf10, out frame);
        }
    }

    public class WebSocketHandshakeBench
    {
        private byte[] _message;
        private ByteBuf _messageByteBuf;

        public WebSocketHandshakeBench()
        {
            _message = Encoding.ASCII.GetBytes(
                "GET /chat HTTP/1.1\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n\r\n"
            );
            _messageByteBuf = new SimpleByteBuf(_message);
        }

        HttpMatcher _httpMatcher = new HttpMatcher();

        [Benchmark]
        public bool Fast()
        {
            bool continueMatching;
            _httpMatcher.Match(_messageByteBuf, out continueMatching);

            var state = _httpMatcher.GetMatchingState;

            return
                state.ConnectionHeaderMatched &
                state.ConnectionHeaderValueMatched &
                state.UpgradeHeaderMatched &
                state.UpgradeHeaderValueMatched &
                state.SecWebSocketVersionHeaderMatched &
                state.SecWebSocketVersionHeaderValueMatched &
                state.SecWebSocketKeyHeaderMatched &
                state.SecWebSocketKeyHeaderValueMatched;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
//            WebSocketHandshakeBench b = new WebSocketHandshakeBench();
//            Console.WriteLine(b.Fast());
//            Console.WriteLine(b.Fast() == b.Old2());
            //var summary = BenchmarkRunner.Run<WebSocketHandshakeBench>();
            //var summary = BenchmarkRunner.Run<WebSocketDecoderBench>();

//            Console.WriteLine(
//                BitByteMagic.GetUShort(127, 56) == ClrByteMagic.GetUShort(127, 56)
//            );
//            Console.WriteLine(
//                BitByteMagic.GetUInt(127, 128, 126, 125) == ClrByteMagic.GetUInt(127, 128, 126, 125)
//            );

            var summary = BenchmarkRunner.Run<WebSocketDecoderBench>();
        }
    }


}