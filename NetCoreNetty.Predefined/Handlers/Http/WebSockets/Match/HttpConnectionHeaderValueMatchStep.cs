namespace NetCoreNetty.Handlers.Http.WebSockets.Match
{
    public class HttpConnectionHeaderValueMatchStep : HttpHeaderValueMatchStepBase
    {
        private IHttpMatchStep _crLfStep;
        private IHttpMatchStep _skipToCrLfStep;
        private IHttpMatchStep _finishStep;

        public HttpConnectionHeaderValueMatchStep()
            : base(HttpHeaderConstants.UpgradeLower, HttpHeaderConstants.UpgradeUpper)
        {
        }

        public void Init(IHttpMatchStep crLfStep, IHttpMatchStep skipToCrLfStep, IHttpMatchStep finishStep)
        {
            _crLfStep = crLfStep;
            _skipToCrLfStep = skipToCrLfStep;
            _finishStep = finishStep;
        }

        protected override void Matched(bool crlf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            state.ConnectionHeaderValueMatched = true;
            newStep = crlf
                ? _crLfStep
                : _skipToCrLfStep;
        }

        protected override void NotMatched(bool crlf, ref HttpMatchState state, out IHttpMatchStep newStep)
        {
            state.ConnectionHeaderValueMatched = false;
            newStep = _finishStep;
        }
    }
}