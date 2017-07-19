namespace NetCoreNetty.Core
{
    internal interface IChannelPipelineInternal : IChannelPipeline
    {
        void EnterInbound();

        void ExitInbound();

        void EnterOutbound();

        void ExitOutbound();
    }
}