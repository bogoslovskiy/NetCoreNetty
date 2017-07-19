using System.Threading.Tasks;

namespace NetCoreNetty.Core
{
    public interface IChannelEventLoop
    {
        void BindInterprocessingQueue(Concurrency.Blocking.BatchBlockingSwap2QueueT<ChannelReadData> queue);

        Task StartListeningAsync();
        
        void Shutdown();
    }
}