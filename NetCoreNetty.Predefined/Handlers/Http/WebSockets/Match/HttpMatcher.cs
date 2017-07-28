using NetCoreNetty.Buffers;

namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    public class HttpMatcher : IHttpMatchStep
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly IHttpMatchStep _httpHeaderNameMatchStep;
        private readonly IHttpMatchStep _skipToCrLfStep;
        private readonly IHttpMatchStep _connectionHeaderValueMatchStep;
        private readonly IHttpMatchStep _upgradeHeaderValueMatchStep;
        private readonly IHttpMatchStep _secWebSocketVersionHeaderValueMatchStep;
        private readonly IHttpMatchStep _secWebSocketKeyHeaderValueMatchStep;
        private readonly IHttpMatchStep _crLfStep;
        private readonly IHttpMatchStep _finishStep;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private IHttpMatchStep _currentStep;
        private HttpMatchState _state;

        public HttpMatchState GetMatchingState {get { return _state; }}

        public HttpMatcher()
        {
            var httpHeaderNameMatchStep = new HttpHeaderNameMatchStep();
            var skipToCrLfStep = new HttpSkipToCrLfStep();
            var connectionHeaderValueMatchStep = new HttpConnectionHeaderValueMatchStep();
            var upgradeHeaderValueMatchStep = new HttpUpgradeHeaderValueMatchStep();
            var secWebSocketVersionHeaderValueMatchStep = new HttpSecWebSocketVersionHeaderValueMatchStep();
            var secWebSocketKeyHeaderValueMatchStep = new HttpSecWebSocketKeyHeaderValueMatchStep();
            var crLfStep = new HttpCrLfStep();

            _httpHeaderNameMatchStep = httpHeaderNameMatchStep;
            _skipToCrLfStep = skipToCrLfStep;
            _connectionHeaderValueMatchStep = connectionHeaderValueMatchStep;
            _upgradeHeaderValueMatchStep = upgradeHeaderValueMatchStep;
            _secWebSocketVersionHeaderValueMatchStep = secWebSocketVersionHeaderValueMatchStep;
            _secWebSocketKeyHeaderValueMatchStep = secWebSocketKeyHeaderValueMatchStep;
            _crLfStep = crLfStep;
            _finishStep = this;

            httpHeaderNameMatchStep.Init(
                _skipToCrLfStep,
                _connectionHeaderValueMatchStep,
                _upgradeHeaderValueMatchStep,
                _secWebSocketVersionHeaderValueMatchStep,
                _secWebSocketKeyHeaderValueMatchStep
            );

            skipToCrLfStep.Init(
                _crLfStep
            );

            connectionHeaderValueMatchStep.Init(
                _crLfStep,
                _skipToCrLfStep,
                _finishStep
            );

            upgradeHeaderValueMatchStep.Init(
                _crLfStep,
                _skipToCrLfStep,
                _finishStep
            );

            secWebSocketVersionHeaderValueMatchStep.Init(
                _crLfStep,
                _skipToCrLfStep,
                _finishStep
            );

            secWebSocketKeyHeaderValueMatchStep.Init(
                _crLfStep
            );

            crLfStep.Init(
                _finishStep,
                _httpHeaderNameMatchStep
            );

            _state = CreateState();

            _currentStep = _skipToCrLfStep;
        }

        public void Clear()
        {
            // TODO: пока что пропускаем первый HTTP метод заголовок.
            _currentStep = _skipToCrLfStep;
            _state.Clear();
        }

        public void Match(ByteBuf byteBuf, out bool continueMatching)
        {
            IHttpMatchStep localNextStep = _currentStep;

            do
            {
                IHttpMatchStep nextStep;
                localNextStep.Match(byteBuf, ref _state, out nextStep);
                // Если необходимо сменить шаг матчинга, то очищаем предыдущий отработавший шаг.
                if (nextStep != null)
                {
                    localNextStep.Clear();
                    // Если следующий шаг сменился, то текущий шаг также переместим на него.
                    _currentStep = nextStep;
                }
                localNextStep = nextStep;
            }
            while (localNextStep != this && localNextStep != null);

            // Если остались на текущем шаге (следующий null), значит обработка
            // в текущем шаге не закончена и в следующий раз надо начать с продолжения в текущем шаге.
            continueMatching = localNextStep == null;
        }

        void IHttpMatchStep.Clear()
        {
        }

        void IHttpMatchStep.Match(ByteBuf byteBuf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            newStep = null;
        }

        private HttpMatchState CreateState()
        {
            // TODO:
            var state = new HttpMatchState();
            state.SecWebSocketKey = new byte[92];

            return state;
        }
    }
}