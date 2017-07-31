namespace NetCoreNetty.Core
{
    abstract public class ChannelPipelineInitializerBase
    {
        abstract protected void Initialize(IChannelPipeline pipeline);

        public IChannelPipeline GetPipeline(ChannelBase channel)
        {
            var pipeline = new ChannelPipeline(channel);
            Initialize(pipeline);

            return pipeline;
        }
    }
}