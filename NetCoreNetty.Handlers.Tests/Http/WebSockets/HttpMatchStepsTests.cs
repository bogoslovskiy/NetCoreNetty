using System.Text;
using NetCoreNetty.Buffers;
using NetCoreNetty.Predefined.Handlers.Http.WebSockets.Match;
using Xunit;

namespace NetCoreNetty.Handlers.Tests.Http.WebSockets
{
    public class HttpMatchStepsTests
    {
        private readonly IHttpMatchStep _httpHeaderNameMatchStep;
        private readonly IHttpMatchStep _skipToCrLfMatchStep;
        private readonly IHttpMatchStep _connectionHeaderValueMatchStep;
        private readonly IHttpMatchStep _upgradeHeaderValueMatchStep;
        private readonly IHttpMatchStep _secWebSocketVersionHeaderValueMatchStep;
        private readonly IHttpMatchStep _secWebSocketKeyHeaderValueMatchStep;
        private readonly IHttpMatchStep _crLfMatchStep;
        private readonly IHttpMatchStep _finishStep;

        public HttpMatchStepsTests()
        {
            _httpHeaderNameMatchStep = new HttpHeaderNameMatchStep();
            _skipToCrLfMatchStep = new HttpSkipToCrLfStep();
            _connectionHeaderValueMatchStep = new HttpConnectionHeaderValueMatchStep();
            _upgradeHeaderValueMatchStep = new HttpUpgradeHeaderValueMatchStep();
            _secWebSocketVersionHeaderValueMatchStep = new HttpSecWebSocketVersionHeaderValueMatchStep();
            _secWebSocketKeyHeaderValueMatchStep = null;
            _crLfMatchStep = new HttpCrLfStep();
            _finishStep = null;

            ((HttpHeaderNameMatchStep) _httpHeaderNameMatchStep).Init(
                _skipToCrLfMatchStep,
                _connectionHeaderValueMatchStep,
                _upgradeHeaderValueMatchStep,
                _secWebSocketVersionHeaderValueMatchStep,
                _secWebSocketKeyHeaderValueMatchStep
            );

            ((HttpSkipToCrLfStep) _skipToCrLfMatchStep).Init(
                _crLfMatchStep
            );

            ((HttpConnectionHeaderValueMatchStep) _connectionHeaderValueMatchStep).Init(
                _crLfMatchStep,
                _skipToCrLfMatchStep,
                _finishStep
            );

            ((HttpCrLfStep)_crLfMatchStep).Init(
                _finishStep,
                _httpHeaderNameMatchStep
            );
        }

        [Fact]
        public void Test1()
        {
            string asciiHttpRequest =
                "Connection: Upgrade";

            var byteBuf = new SimpleByteBuf(Encoding.ASCII.GetBytes(asciiHttpRequest));

            HttpMatchState state = default(HttpMatchState);

            IHttpMatchStep nextStep;
            _httpHeaderNameMatchStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.ConnectionHeaderMatched);
            Assert.True(nextStep == _connectionHeaderValueMatchStep);
        }

        [Fact]
        public void Test2()
        {
            string asciiHttpRequest =
                "Connection: Upgrade\r\nSome-header: 123ghtyry\r\nUpgrade: WebSocketes";

            var byteBuf = new SimpleByteBuf(Encoding.ASCII.GetBytes(asciiHttpRequest));

            HttpMatchState state = default(HttpMatchState);

            IHttpMatchStep nextStep;
            _httpHeaderNameMatchStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.ConnectionHeaderMatched);
            Assert.True(nextStep == _connectionHeaderValueMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.ConnectionHeaderValueMatched);
            Assert.True(nextStep == _crLfMatchStep);

            // *****
            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _httpHeaderNameMatchStep);

            _httpHeaderNameMatchStep.Clear();
            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _skipToCrLfMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _crLfMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            _httpHeaderNameMatchStep.Clear();
            Assert.True(nextStep == _httpHeaderNameMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.UpgradeHeaderMatched);
            Assert.True(nextStep == _upgradeHeaderValueMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.False(state.UpgradeHeaderValueMatched);
            Assert.True(nextStep == null);
        }

        [Fact]
        public void Test3()
        {
            string asciiHttpRequest =
                "Connection: keep-alive, upgrade\r\nSome-header: 123ghtyry\r\nUpgrade: WebSocketes";

            var byteBuf = new SimpleByteBuf(Encoding.ASCII.GetBytes(asciiHttpRequest));

            HttpMatchState state = default(HttpMatchState);

            IHttpMatchStep nextStep;
            _httpHeaderNameMatchStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.ConnectionHeaderMatched);
            Assert.True(nextStep == _connectionHeaderValueMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.ConnectionHeaderValueMatched);
            Assert.True(nextStep == _crLfMatchStep);

            // *****
            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _httpHeaderNameMatchStep);

            _httpHeaderNameMatchStep.Clear();
            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _skipToCrLfMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _crLfMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            _httpHeaderNameMatchStep.Clear();
            Assert.True(nextStep == _httpHeaderNameMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.UpgradeHeaderMatched);
            Assert.True(nextStep == _upgradeHeaderValueMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.False(state.UpgradeHeaderValueMatched);
            Assert.True(nextStep == null);
        }

        [Fact]
        public void Test4()
        {
            string asciiHttpRequest =
                "Connection: upgrade, keep-alive\r\nSome-header: 123ghtyry\r\nUpgrade: WebSocketes";

            var byteBuf = new SimpleByteBuf(Encoding.ASCII.GetBytes(asciiHttpRequest));

            HttpMatchState state = default(HttpMatchState);

            IHttpMatchStep nextStep;
            _httpHeaderNameMatchStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.ConnectionHeaderMatched);
            Assert.True(nextStep == _connectionHeaderValueMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.ConnectionHeaderValueMatched);
            Assert.True(nextStep == _skipToCrLfMatchStep);

            // *****
            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _crLfMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _httpHeaderNameMatchStep);

            _httpHeaderNameMatchStep.Clear();
            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _skipToCrLfMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(nextStep == _crLfMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            _httpHeaderNameMatchStep.Clear();
            Assert.True(nextStep == _httpHeaderNameMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.True(state.UpgradeHeaderMatched);
            Assert.True(nextStep == _upgradeHeaderValueMatchStep);

            nextStep.Match(byteBuf, ref state, out nextStep);

            Assert.False(state.UpgradeHeaderValueMatched);
            Assert.True(nextStep == null);
        }
    }
}