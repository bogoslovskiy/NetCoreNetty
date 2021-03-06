﻿using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    internal class ChannelHandlerContext : IChannelHandlerContext
    {
        private ChannelBase _channel;
        private IChannelHandler _handler;
        private IChannelPipeline _pipeline;
        
        internal ChannelHandlerContext Next;
        internal ChannelHandlerContext Prev;

        public ChannelBase Channel => _channel;

        public IChannelHandler Handler => _handler;

        public IChannelPipeline Pipeline => _pipeline;

        public IByteBufAllocator ChannelByteBufAllocator => _channel.ByteBufAllocator;

        public string Name { get; }

        public ChannelHandlerContext(
            string name,
            ChannelBase channel,
            IChannelHandler handler,
            IChannelPipeline pipeline)
        {
            this.Name = name;
            _channel = channel;
            _handler = handler;
            _pipeline = pipeline;
        }

        public void Read(object message)
        {
            Next.InvokeRead(message);
        }

        public void Write(object message)
        {
            Prev.InvokeWrite(message);
        }

        internal void InvokeRead(object message)
        {
            try
            {
                InvokeReadWithLock(message);
            }
            catch (Exception ex)
            {
                // TODO:
                Console.WriteLine(ex);
            }
        }

        internal void InvokeWrite(object message)
        {
            try
            {
                InvokeWriteWithLock(message);
            }
            catch (Exception ex)
            {
                // TODO:
                Console.WriteLine(ex);
            }
        }

        private void InvokeReadWithLock(object message)
        {
            try
            {
                _handler.Read(this, message);
            }
            catch (Exception ex)
            {
                // TODO:
                Console.WriteLine(ex);
            }
            finally
            {
            }
        }
        
        private void InvokeWriteWithLock(object message)
        {
            try
            {
                _handler.Write(this, message);
            }
            catch (Exception ex)
            {
                // TODO:
                Console.WriteLine(ex);
            }
            finally
            {
            }
        }

        internal void LinkAfter(ChannelHandlerContext ctx)
        {
            Link(ctx, ctx.Next);
        }

        internal void LinkBefore(ChannelHandlerContext ctx)
        {
            Link(ctx.Prev, ctx);
        }

        internal void LinkReplace(ChannelHandlerContext ctx)
        {
            // TODO: освобождение предыдущих, если пулится
            Link(ctx.Prev, ctx.Next);
        }
        
        private void Link(ChannelHandlerContext prev, ChannelHandlerContext next)
        {
            Prev = prev;
            if (prev != null)
            {
                prev.Next = this;
            }

            Next = next;
            if (next != null)
            {
                next.Prev = this;
            }
        }
    }
}