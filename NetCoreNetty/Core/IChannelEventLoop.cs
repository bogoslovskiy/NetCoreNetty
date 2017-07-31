using System.Threading.Tasks;

namespace NetCoreNetty.Core
{
    public interface IChannelEventLoop
    {
        void Bind(IInboundBuffer inboundBuffer);
        
        Task StartListeningAsync();
        
        void Shutdown();
    }
}