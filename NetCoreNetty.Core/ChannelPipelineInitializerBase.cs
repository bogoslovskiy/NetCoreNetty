using NetCoreNetty.Core.Executors;

namespace NetCoreNetty.Core
{
    abstract public class ChannelPipelineInitializerBase
    {
        protected virtual IExecutor CreateExecutor()
        {
            return new DefaultSameThreadExecutor();
        }
        
        abstract protected void Initialize(IChannelPipeline pipeline);

        public IChannelPipeline GetPipeline(IChannel channel)
        {
            IExecutor executor = CreateExecutor();
            
            var pipeline = new ChannelPipeline(channel, executor);
            Initialize(pipeline);

            return pipeline;
        }
    }
}