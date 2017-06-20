using System.Collections.Generic;
using System.Threading.Tasks;
using NetCoreNetty.Buffers.Unmanaged;
using NetCoreNetty.Core;
using NetCoreNetty.Libuv;

namespace NetCoreNetty.Channels
{
    public class LibuvEventLoop : LibuvLoopHandle
    {
        private readonly IUnmanagedByteBufAllocator _channelByteBufAllocator;
        private readonly ChannelPipelineInitializerBase _channelPipelineInitializer;
        private readonly string _hostUrl;

        static private LibuvTcpHandle _serverTcpHandle;
        static private List<LibuvTcpHandle> _clientTcpHandles = new List<LibuvTcpHandle>();

        public LibuvEventLoop(
            IUnmanagedByteBufAllocator channelByteBufAllocator,
            ChannelPipelineInitializerBase channelPipelineInitializer,
            string hostUrl)
        {
            _channelByteBufAllocator = channelByteBufAllocator;
            _channelPipelineInitializer = channelPipelineInitializer;
            _hostUrl = hostUrl;
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
            _serverTcpHandle.Listen(100 /* backLog */, ConnectionCallback);

            Run(0 /* mode = Default */);
        }

        private void ConnectionCallback(LibuvStreamHandle streamHandle, int status)
        {
            var libuvTcpServerChannel = new LibuvTcpServerChannel(_channelByteBufAllocator);
            libuvTcpServerChannel.Init(this);

            streamHandle.Accept(libuvTcpServerChannel);
            _clientTcpHandles.Add(libuvTcpServerChannel);

            // TODO: тут все принципы проектирования нарушены. по возможности порефакторить.
            IChannelPipeline pipeline = _channelPipelineInitializer.GetPipeline(libuvTcpServerChannel);

            libuvTcpServerChannel.StartRead(pipeline.ChannelReadCallback);
        }
    }
}