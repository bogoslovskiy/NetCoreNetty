using System;

namespace NetCoreNetty.Core
{
    public class LambdaChannelPipelineInitializer : ChannelPipelineInitializerBase
    {
        private readonly Action<IChannelPipeline> _initializeAction;

        public LambdaChannelPipelineInitializer(Action<IChannelPipeline> initializeAction)
        {
            _initializeAction = initializeAction;
        }

        protected override void Initialize(IChannelPipeline pipeline)
        {
            _initializeAction(pipeline);
        }
    }
}