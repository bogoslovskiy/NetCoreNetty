﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreNetty.Concurrency;
using NetCoreNetty.Core;
using NetCoreNetty.Predefined.Buffers.Unmanaged;
using NetCoreNetty.Predefined.Channels.Libuv;
using NetCoreNetty.Predefined.Codecs.WebSockets;
using NetCoreNetty.Predefined.Handlers.Http.WebSockets;

namespace NetCoreNetty.Sandbox
{
    internal class Program
    {
        class LogWebSocketFrameHandler : ChannelHandlerBase
        {
            public override void Read(IChannelHandlerContext ctx, object input)
            {
                var frame = input as WebSocketFrame;
                if (frame != null)
                {
                    Console.WriteLine("[* ws frame {0} '{1}' *]", frame.Type, frame.Text);
                }

                // Проброс.
                ctx.Read(input);
            }
        }

        class WebSocketEchoHandler : ChannelHandlerBase
        {
            public override void Read(IChannelHandlerContext ctx, object input)
            {
                var frame = input as WebSocketFrame;
                if (frame != null)
                {
                    if (frame.Type == WebSocketFrameType.Ping)
                    {
                        ctx.Write(new WebSocketFrame {Type = WebSocketFrameType.Pong, IsFinal = true});
                    }
                    else if (frame.Type == WebSocketFrameType.Close)
                    {
                        ctx.Write(
                            new WebSocketFrame
                            {
                                Type = WebSocketFrameType.Close,
                                IsFinal = true,
                                Bytes = Encoding.UTF8.GetBytes("1000")
                            }
                        );
                    }
                    else if (frame.Type == WebSocketFrameType.Text)
                    {
                        string inboundMessage = frame.Text;
                        string echo = string.Format("Echo: '{0}'", inboundMessage);

                        ctx.Write(
                            new WebSocketFrame
                            {
                                Type = WebSocketFrameType.Text,
                                IsFinal = true,
                                Bytes = Encoding.UTF8.GetBytes(echo)
                            }
                        );
                    }
                }
            }
        }

        static public void Main(string[] args)
        {
            var fastHttpWebSocketHandshakeHandlerProvider = new FastHttpWebSocketHandshakeHandlerProvider(
                "webSockets13",
                new DefaultChannelHandlerProvider<WebSocketDecoder>(),
                new DefaultChannelHandlerProvider<WebSocketEncoder>()
            );

            ChannelPipelineInitializerBase channelPipelineInitializer = new LambdaChannelPipelineInitializer(
                (pipeline) =>
                {
                    pipeline.AddLast(
                        "fastHttpWebSocketHandshake",
                        fastHttpWebSocketHandshakeHandlerProvider
                    );
                    pipeline.AddLast<LogWebSocketFrameHandler>("webSocketsLog");
                    pipeline.AddLast<WebSocketEchoHandler>("webSocketsEcho");
                }
            );

            IUnmanagedByteBufAllocator unmanagedByteBufAllocator =
                new UnmanagedByteBufAllocator();

            LibuvEventLoop eventLoop = new LibuvEventLoop(
                unmanagedByteBufAllocator,
                channelPipelineInitializer,
                "http://127.0.0.1:5052",
                100 /* listenBacklog */
            );

            var produceConsumeBuffer = new FastProduceConsumeBuffer<ChannelReadData>(8);

            Thread consumerThread = StartConsumerThread(produceConsumeBuffer);
            
            eventLoop.Bind(produceConsumeBuffer);
            
            Task listeningTask = eventLoop.StartListeningAsync();

            Console.WriteLine("Listening 5052 ...");
            Console.WriteLine("Press any key to stop listening...");
            Console.ReadLine();

            eventLoop.Shutdown();
        }

        static private Thread StartConsumerThread(FastProduceConsumeBuffer<ChannelReadData> produceConsumeBuffer)
        {
            var th = new Thread(
                obj =>
                {
                    var buffer = (FastProduceConsumeBuffer<ChannelReadData>)obj;
                    buffer.StartRead(ProcessChannelData);
                }
            );
            th.Start(produceConsumeBuffer);

            return th;
        }
        
        static private void ProcessChannelData(ChannelReadData data)
        {
            data.ChannelPipeline.ChannelReadCallback(data.ChannelPipeline.Channel, data.ByteBuffer);
        }
    }
}