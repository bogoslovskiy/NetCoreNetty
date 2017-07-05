using System;
using System.Threading;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Core
{
    internal class ChannelPipeline : IChannelPipelineInternal
    {
        class TerminatorChannelHandler : ChannelHandlerBase
        {
            public override void Read(IChannelHandlerContext ctx, object message)
            {
                ctx.Read(message);
            }

            public override void Write(IChannelHandlerContext ctx, object message)
            {
                if (message is ByteBuf == false)
                {
                    throw new InvalidOperationException();
                }

                ctx.Channel.Write((ByteBuf)message);
            }
        }

        private IChannel _channel;
        private IExecutor _executor;
        private ChannelHandlerContext _terminatorHandlerContext;
        
        private int _inboundThreadId = -1;
        private int _outboundThreadId = -1;
        
        public IExecutor Executor => _executor;

        public ChannelPipeline(
            IChannel channel,
            IExecutor executor)
        {
            _channel = channel;
            _executor = executor;
            _terminatorHandlerContext = CreateContext(
                "_firstHandlerContext",
                new TerminatorChannelHandler()
            );
        }
        
        public void EnterInbound()
        {
            // Блокировку в любом случае надо взять.
            while (true)
            {
                // Неблокирующая синхронизация.
                // Устанавливаем текущий поток, если inbound поток не установлен (-1).
                int inboundThreadId = Interlocked.CompareExchange(
                    ref _inboundThreadId,
                    Thread.CurrentThread.ManagedThreadId,
                    -1
                );
            
                // Если inboundThreadId (старое значение перед попыткой установки)
                // равен -1, то мы установили текущий поток владельцем блокировки,
                // если значение равно идентификатору текущего потока, то значение было установлено раньше и 
                // текущий поток итак был владельцем.
                if (inboundThreadId == -1 | inboundThreadId == Thread.CurrentThread.ManagedThreadId)
                {
                    // Текущий поток либо итак владелец блокировки, либо взял блокировку.
                    return;
                }
                
                // Засыпаем, если не удалось взять блокировку. Т.к. мы не знаем, когда блокировка освободится,
                // спать будем недолго, чтобы как можно быстрее взять блокировку при первой же возможности.
                // TODO: возможно нужен свой CustomSpinWait, где вести статистику ожидания и менять время засыпания.
                // TODO: стандартный SpinWait тут не подходит, т.к. он делает Thread.Yield, 
                // TODO: что означает передачу управления другому потоку, что недопустимо
                Thread.Sleep(0);
            }
        }

        public void ExitInbound()
        {
            // Неблокирующая синхронизация.
            // Устанавливаем inbound поток в -1, если он был равен текущему.
            int inboundThreadId = Interlocked.CompareExchange(
                ref _inboundThreadId,
                -1,
                Thread.CurrentThread.ManagedThreadId
            );
            
            // Если была попытка выйти из блокировки, не владея ей, кидаем исключение.
            if (inboundThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException("Current thread is not owner of read lock.");
            }
        }

        public void EnterOutbound()
        {
            // Блокировку в любом случае надо взять.
            while (true)
            {
                // Неблокирующая синхронизация.
                // Устанавливаем текущий поток, если inbound поток не установлен (-1).
                int outboundThreadId = Interlocked.CompareExchange(
                    ref _outboundThreadId,
                    Thread.CurrentThread.ManagedThreadId,
                    -1
                );
            
                // Если outboundThreadId (старое значение перед попыткой установки)
                // равен -1, то мы установили текущий поток владельцем блокировки,
                // если значение равно идентификатору текущего потока, то значение было установлено раньше и 
                // текущий поток итак был владельцем.
                if (outboundThreadId == -1 | outboundThreadId == Thread.CurrentThread.ManagedThreadId)
                {
                    // Текущий поток либо итак владелец блокировки, либо взял блокировку.
                    return;
                }
                
                // Засыпаем, если не удалось взять блокировку. Т.к. мы не знаем, когда блокировка освободится,
                // спать будем недолго, чтобы как можно быстрее взять блокировку при первой же возможности.
                // TODO: возможно нужен свой CustomSpinWait, где вести статистику ожидания и менять время засыпания.
                // TODO: стандартный SpinWait тут не подходит, т.к. он делает Thread.Yield, 
                // TODO: что означает передачу управления другому потоку, что недопустимо
                Thread.Sleep(0);
            }
        }

        public void ExitOutbound()
        {
            // Неблокирующая синхронизация.
            // Устанавливаем outbound поток в -1, если он был равен текущему.
            int outboundThreadId = Interlocked.CompareExchange(
                ref _outboundThreadId,
                -1,
                Thread.CurrentThread.ManagedThreadId
            );
            
            // Если была попытка выйти из блокировки, не владея ей, кидаем исключение.
            if (outboundThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException("Current thread is not owner of write lock.");
            }
        }

        public void StartReceiving()
        {
            _channel.StartRead(ChannelReadCallback);
        }

        public void StopReceiving()
        {
            _channel.StopRead();
        }

        public void AddFirst<TChannelHandler>(string name)
            where TChannelHandler : IChannelHandler, new()
        {
            IChannelHandler handler = new TChannelHandler();
            AddFirst(name, handler);
        }

        public void AddLast<TChannelHandler>(string name)
            where TChannelHandler : IChannelHandler, new()
        {
            IChannelHandler handler = new TChannelHandler();
            AddLast(name, handler);
        }

        public void AddLast(string name, IChannelHandlerProvider channelHandlerProvider)
        {
            IChannelHandler handler = channelHandlerProvider.GetHandler();
            AddLast(name, handler);
        }

        public void AddBefore<TChannelHandler>(string targetName, string name)
            where TChannelHandler : IChannelHandler, new()
        {
            IChannelHandler handler = new TChannelHandler();
            AddBefore(targetName, name, handler);
        }

        public void AddBefore(string targetName, string name, IChannelHandlerProvider channelHandlerProvider)
        {
            IChannelHandler handler = channelHandlerProvider.GetHandler();
            AddBefore(targetName, name, handler);
        }

        public void Replace<TChannelHandler>(string targetName, string name)
            where TChannelHandler : IChannelHandler, new()
        {
            IChannelHandler handler = new TChannelHandler();
            Replace(targetName, name, handler);
        }

        public void Replace(string targetName, string name, IChannelHandlerProvider channelHandlerProvider)
        {
            IChannelHandler handler = channelHandlerProvider.GetHandler();
            Replace(targetName, name, handler);
        }

        public void ChannelReadCallback(IChannel channel, ByteBuf byteBuf)
        {
            _terminatorHandlerContext.InvokeRead(byteBuf);
        }

        private void AddFirst(string name, IChannelHandler handler)
        {
            ChannelHandlerContext ctx = CreateContext(name, handler);

            ctx.LinkAfter(_terminatorHandlerContext);
        }

        private void AddLast(string name, IChannelHandler handler)
        {
            ChannelHandlerContext ctx = CreateContext(name, handler);

            ctx.LinkAfter(GetLastContext());
        }

        private void AddBefore(string targetName, string name, IChannelHandler handler)
        {
            ChannelHandlerContext ctx = CreateContext(name, handler);

            ChannelHandlerContext targetCtx = FindContextByName(targetName);
            ctx.LinkBefore(targetCtx);
        }

        private void Replace(string targetName, string name, IChannelHandler handler)
        {
            ChannelHandlerContext ctx = CreateContext(name, handler);

            ChannelHandlerContext ctxToReplace = FindContextByName(targetName);
            ctx.LinkReplace(ctxToReplace);
        }

        private ChannelHandlerContext CreateContext(string name, IChannelHandler handler)
        {
            var ctx = new ChannelHandlerContext(name, _channel, handler, this);
            return ctx;
        }

        private ChannelHandlerContext GetLastContext()
        {
            ChannelHandlerContext ctx = _terminatorHandlerContext;
            while (ctx.Next != null)
            {
                ctx = ctx.Next;
            }

            return ctx;
        }

        private ChannelHandlerContext FindContextByName(string name)
        {
            ChannelHandlerContext ctx = _terminatorHandlerContext.Next;
            while (!string.Equals(ctx.Name, name))
            {
                ctx = ctx.Next;
            }

            if (ctx == null)
            {
                throw new InvalidOperationException();
            }

            return ctx;
        }
    }
}