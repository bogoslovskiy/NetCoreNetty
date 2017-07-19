using System.Threading.Tasks;
using NetCoreNetty.Concurrency.Blocking;

namespace NetCoreNetty.Core
{
    public class ChannelPipelineExecutor
    {
        private BatchBlockingSwap2QueueT<ChannelReadData> _interprocessingQueue;

        public ChannelPipelineExecutor(BatchBlockingSwap2QueueT<ChannelReadData> interprocessingQueue)
        {
            _interprocessingQueue = interprocessingQueue;
        }

        public async Task StartPipelinesProcessing()
        {
            await Task.Factory.StartNew(Start);
        }

        private void Start()
        {
            while (true)
            {
                _interprocessingQueue.EnterRead();

                ChannelReadData channelReadData;
                while (_interprocessingQueue.TryDequeue(out channelReadData))
                {
                    channelReadData.ChannelPipeline.ChannelReadCallback(
                        channelReadData.ChannelPipeline.Channel,
                        channelReadData.ByteBuffer
                    );
                }
                
                _interprocessingQueue.ExitRead();
            }
        }
    }
}