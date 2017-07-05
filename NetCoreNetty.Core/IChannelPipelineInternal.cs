namespace NetCoreNetty.Core
{
    internal interface IChannelPipelineInternal : IChannelPipeline
    {
        IExecutor Executor { get; }

        void EnterInbound();

        void ExitInbound();

        void EnterOutbound();

        void ExitOutbound();
    }
}