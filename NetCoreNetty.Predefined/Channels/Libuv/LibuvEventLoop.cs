using System;
using System.Collections.Generic;
using NetCoreNetty.Core;
using NetCoreNetty.Libuv;
using NetCoreNetty.Predefined.Buffers.Unmanaged;

namespace NetCoreNetty.Predefined.Channels.Libuv
{
    public class LibuvEventLoop : ChannelEventLoopBase<LibuvEventLoopOptions>
    {
        private readonly IUnmanagedByteBufAllocator _channelByteBufAllocator;
        private readonly ChannelPipelineInitializerBase _channelPipelineInitializer;

        public LibuvLoopHandle LibuvLoopHandle { get; private set; }
        private LibuvAsyncHandle _closeLoopHandle;
        private LibuvTcpHandle _serverTcpHandle;
        private List<LibuvTcpServerChannel> _clientTcpHandles = new List<LibuvTcpServerChannel>();

        public LibuvEventLoop(
            IUnmanagedByteBufAllocator channelByteBufAllocator,
            ChannelPipelineInitializerBase channelPipelineInitializer,
            IInboundBuffer inboundBuffer)
            : base(inboundBuffer)
        {
            _channelByteBufAllocator = channelByteBufAllocator;
            _channelPipelineInitializer = channelPipelineInitializer;
        }

        protected override void StartCore()
        {
            if (LibuvLoopHandle != null)
            {
                throw new Exception();
            }
            
            LibuvLoopHandle = new LibuvLoopHandle();
            LibuvLoopHandle.Init();
            
            _closeLoopHandle = new LibuvAsyncHandle();
            _closeLoopHandle.Init(LibuvLoopHandle, StopLoopCallback);
            
            _serverTcpHandle = new LibuvTcpHandle();
            _serverTcpHandle.Init(LibuvLoopHandle);

            _serverTcpHandle.Bind(ServerAddress.FromUrl(this.Options.Uri));
            _serverTcpHandle.Listen(this.Options.ListenBacklog /* backLog */, ConnectionCallback);
            
            LibuvLoopHandle.RunDefault();
        }

        protected override void StopCore()
        {
            if (_closeLoopHandle == null)
            {
                throw new InvalidOperationException();
            }
            
            // Signal to the loop to stop it with associated async handle callback.
            _closeLoopHandle.Send();
        }

        protected override void InitializeDefaultOptions(LibuvEventLoopOptions options)
        {
            options.Uri = "http://127.0.0.1:5052";
            options.ListenBacklog = 100;
        }

        private void StopLoopCallback(IntPtr asyncHandle)
        {
            // TODO: !!!
            // Close all clientTcpHandles.
            // Stop loop
            // Close loop
            // Dispose
        }
        
        private void ConnectionCallback(LibuvStreamHandle streamHandle, int status)
        {
            var libuvTcpServerChannel = new LibuvTcpServerChannel(
                this,
                _channelByteBufAllocator,
                InboundBuffer
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