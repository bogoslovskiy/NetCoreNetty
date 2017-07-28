using System.Collections.Generic;
using System.Threading.Tasks;
using NetCoreNetty.Buffers;
using NetCoreNetty.Buffers.Unmanaged;
using NetCoreNetty.Channels;
using NetCoreNetty.Concurrency;
using NetCoreNetty.Core;
using NetCoreNetty.Libuv;

namespace NetCoreNetty.Predefined.Channels.Libuv
{
    public class LibuvEventLoop : LibuvLoopHandle, IChannelEventLoop
    {
        private readonly IUnmanagedByteBufAllocator _channelByteBufAllocator;
        private readonly ChannelPipelineInitializerBase _channelPipelineInitializer;
        private readonly string _hostUrl;
        private readonly int _listenBacklog;
        
        private LibuvTcpHandle _serverTcpHandle;
        private List<LibuvTcpHandle> _clientTcpHandles = new List<LibuvTcpHandle>();

        private FastProduceConsumeBuffer<ChannelReadData> _buffer;
        
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

        public void Bind(FastProduceConsumeBuffer<ChannelReadData> buffer)
        {
            _buffer = buffer;
        }

        public async Task StartListeningAsync()
        {
            await Task.Factory.StartNew(StartListening);
        }

        public void Shutdown()
        {
            Stop();
        }

        internal void EnqueueReadedData(IChannelPipeline pipeline, ByteBuf byteBuffer)
        {
            // Пишем.
            var channelReadData = new ChannelReadData(pipeline, byteBuffer);

            _buffer.Write(pipeline.ExecutionBuffer, channelReadData);
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
            var libuvTcpServerChannel = new LibuvTcpServerChannel(this, _channelByteBufAllocator);

            // TODO: тут все принципы проектирования нарушены. по возможности порефакторить.
            IChannelPipeline pipeline = _channelPipelineInitializer.GetPipeline(libuvTcpServerChannel);
            libuvTcpServerChannel.Pipeline = pipeline;
            
            streamHandle.Accept(libuvTcpServerChannel);
            _clientTcpHandles.Add(libuvTcpServerChannel);

            libuvTcpServerChannel.StartRead();
        }
    }
}