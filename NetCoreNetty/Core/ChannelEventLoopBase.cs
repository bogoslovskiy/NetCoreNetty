using System.Threading.Tasks;

namespace NetCoreNetty.Core
{
    abstract public class ChannelEventLoopBase<TOptions>
        where TOptions : class, new()
    {
        protected IInboundBuffer InboundBuffer { get; }
        
        protected TOptions Options { get; }

        private Task _executionTask;

        protected ChannelEventLoopBase(IInboundBuffer inboundBuffer)
        {
            this.InboundBuffer = inboundBuffer;
            
            this.Options = new TOptions();
        }
        
        abstract protected void StartCore();
        
        abstract protected void StopCore();

        abstract protected void InitializeDefaultOptions(TOptions options);

        public Task StartTask()
        {
            InitializeDefaultOptions(this.Options);
            
            _executionTask = Task.Factory.StartNew(StartCore);
            return _executionTask;
        }

        public void Stop()
        {
            StopCore();
        }
    }
}