namespace NetCoreNetty.Predefined.Handlers.Http.WebSockets.Match
{
    public class HttpSecWebSocketVersionHeaderValueMatchStep : HttpHeaderValueMatchStepBase
    {
        private IHttpMatchStep _crLfStep;
        private IHttpMatchStep _skipToCrLfStep;
        private IHttpMatchStep _finishStep;

        public HttpSecWebSocketVersionHeaderValueMatchStep()
            : base(HttpHeaderConstants.Version13, HttpHeaderConstants.Version13)
        {
        }

        public void Init(IHttpMatchStep crLfStep,IHttpMatchStep skipToCrLfStep, IHttpMatchStep finishStep)
        {
            _crLfStep = crLfStep;
            _skipToCrLfStep = skipToCrLfStep;
            _finishStep = finishStep;
        }

        protected override void Matched(bool crlf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            state.SecWebSocketVersionHeaderValueMatched = true;
            newStep = crlf
                ? _crLfStep
                : _skipToCrLfStep;
        }

        protected override void NotMatched(bool crlf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            state.SecWebSocketVersionHeaderValueMatched = false;
            newStep = _finishStep;
        }
    }
}