using System.Collections.Generic;
using System.Threading.Tasks;
using NetCoreNetty.Core;
using NetCoreNetty.Libuv;
using NetCoreNetty.Predefined.Buffers.Unmanaged;

namespace NetCoreNetty.Predefined.Channels.Libuv
{
    public class LibuvEventLoop : LibuvLoopHandle, IChannelEventLoop
    {
        private readonly IUnmanagedByteBufAllocator _channelByteBufAllocator;
        private readonly ChannelPipelineInitializerBase _channelPipelineInitializer;
        private readonly string _hostUrl;
        private readonly int _listenBacklog;
        
        private LibuvTcpHandle _serverTcpHandle;
        private List<LibuvTcpServerChannel> _clientTcpHandles = new List<LibuvTcpServerChannel>();

        private IInboundBuffer _inboundBuffer;
        
        public LibuvEventLoop(
            IUnmanagedByteBufAllocator channelByteBufAllocator,
            ChannelPipelineInitializerBase channelPipelineInitializer,
            string hostUrl,
            int listenBacklog)
        {
            _channelByteBufAllocator = channelByteBufAllocator;
            _channelPipelineInitializer = channelPipelineInitializer;
            _hostUrl = hostUrl;
            _listenBacklog = listenBacklog;
        }

        public void Bind(IInboundBuffer inboundBuffer)
        {
            _inboundBuffer = inboundBuffer;
        }

        public async Task StartListeningAsync()
        {
            await Task.Factory.StartNew(StartListening);
        }

        public void Shutdown()
        {
            Stop();
        }

        private void StartListening()
        {
            Init();

            _serverTcpHandle = new LibuvTcpHandle();
            _serverTcpHandle.Init(this);

            _serverTcpHandle.Bind(ServerAddress.FromUrl(_hostUrl));
            _serverTcpHandle.Listen(_listenBacklog /* backLog */, ConnectionCallback);
            
            RunDefault();
        }

        private void ConnectionCallback(LibuvStreamHandle streamHandle, int status)
        {
            var libuvTcpServerChannel = new LibuvTcpServerChannel(
                this,
                _channelByteBufAllocator,
                _inboundBuffer
            );

            // TODO: тут все принципы проектирования нарушены. по возможности порефакторить.
            IChannelPipeline pipeline = _channelPipelineInitializer.GetPipeline(libuvTcpServerChannel);
            libuvTcpServerChannel.ChannelPipeline = pipeline;
            
            streamHandle.Accept(libuvTcpServerChannel.LibuvTcpHandle);
            _clientTcpHandles.Add(libuvTcpServerChannel);

            libuvTcpServerChannel.StartRead();
        }
    }
}