using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetCoreNetty.Buffers;
using NetCoreNetty.Buffers.Unmanaged;
using NetCoreNetty.Core;
using NetCoreNetty.Libuv;

namespace NetCoreNetty.Channels
{
    public class LibuvEventLoop : LibuvLoopHandle, IChannelEventLoop
    {
        private readonly IUnmanagedByteBufAllocator _channelByteBufAllocator;
        private readonly ChannelPipelineInitializerBase _channelPipelineInitializer;
        private readonly string _hostUrl;
        private readonly int _listenBacklog;
        
        private LibuvTcpHandle _serverTcpHandle;
        private List<LibuvTcpHandle> _clientTcpHandles = new List<LibuvTcpHandle>();
        private LibuvPrepareHandle _prepareHandle;

        private Concurrency.Blocking.BatchBlockingSwap2QueueT<ChannelReadData> _interprocessingQueue;
        
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

        public void BindInterprocessingQueue(
            Concurrency.Blocking.BatchBlockingSwap2QueueT<ChannelReadData> interprocessingQueue)
        {
            if (_interprocessingQueue == null)
            {
                _interprocessingQueue = interprocessingQueue;
            }
            else
            {
                throw new InvalidOperationException("Interprocessing queue alreade bound.");
            }
        }

        public async Task StartListeningAsync()
        {
            await Task.Factory.StartNew(StartListening);
        }

        public void Shutdown()
        {
            Stop();
        }

        private bool _writeEntered;
        private int _writes;

        internal void EnqueueReadedData(IChannelPipeline pipeline, ByteBuf byteBuffer)
        {
            // Пишем.
            var channelReadData = new ChannelReadData(pipeline, byteBuffer);

            bool writed = _interprocessingQueue.TryEnqueue(channelReadData);
            if (!writed)
            {
                // Не удалось записать в очередь. Это может означать только одно: дальше писать нельзя.
                // Надо закрыть блокировку на пакетную запись и открыть ее заново.
                SwapQueueBuffers();
                
                // Пробуем записать еще раз. Если не получится, то какие-то проблемы с очередью, такого быть не должно.
                writed = _interprocessingQueue.TryEnqueue(channelReadData);
                if (!writed)
                {
                    throw new Exception("Something wrong with interprocessing queue.");
                }
            }
            
            _writes++;
        }

        internal void PrepareCallback(IntPtr prepareHandle)
        {
            // Если ранее не открывали блокировку на запись, то сделаем это.
            if (!_writeEntered)
            {
                _interprocessingQueue.EnterWrite();
                _writeEntered = true;
            }
            else
            {
                // Если блокировка на запись уже была открыта, то смотрим, были ли записи.
                // Если записи были, то отпускаем блокировку и пробуем взять следующую, в этот момент
                // очередь обменяет буферы, если сможет.
                if (_writes > 0)
                {
                    SwapQueueBuffers();
                }
            }
        }

        private void SwapQueueBuffers()
        {
            _interprocessingQueue.ExitWrite();
            // Поток будет заблокирован здесь до тех пор, пока очередь не будет свободна для записи.
            _interprocessingQueue.EnterWrite();

            _writes = 0;
            _writeEntered = true;
        }

        private void StartListening()
        {
            Init();

            _serverTcpHandle = new LibuvTcpHandle();
            _serverTcpHandle.Init(this);

            _serverTcpHandle.Bind(ServerAddress.FromUrl(_hostUrl));
            _serverTcpHandle.Listen(_listenBacklog /* backLog */, ConnectionCallback);

            _prepareHandle = new LibuvPrepareHandle();
            _prepareHandle.Init(this);
            
            RunLoop();
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

        private void RunLoop()
        {
            _prepareHandle.Start(PrepareCallback /* prepareCallback */);

            RunDefault();
            
            _prepareHandle.Stop();
        }
    }
}