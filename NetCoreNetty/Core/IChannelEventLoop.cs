using System.Threading.Tasks;

namespace NetCoreNetty.Core
{
    public interface IChannelEventLoop
    {
        void Bind(Concurrency.FastProduceConsumeBuffer<ChannelReadData> buffer);
        
        Task StartListeningAsync();
        
        void Shutdown();
    }
}