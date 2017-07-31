using System;
using System.Runtime.InteropServices;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;
using NetCoreNetty.Libuv;
using NetCoreNetty.Predefined.Buffers.Unmanaged;

namespace NetCoreNetty.Predefined.Channels.Libuv
{
    public class LibuvTcpServerChannel : ChannelBase
    {
        private readonly IUnmanagedByteBufAllocator _byteBufAllocator;

        internal LibuvTcpHandle LibuvTcpHandle { get; }

        public LibuvTcpServerChannel(
            LibuvEventLoop libuvEventLoop,
            IUnmanagedByteBufAllocator byteBufAllocator,
            IInboundBuffer inboundBuffer)
            : base(byteBufAllocator, inboundBuffer)
        {
            _byteBufAllocator = byteBufAllocator;

            LibuvTcpHandle = new LibuvTcpHandle();
            LibuvTcpHandle.Init(libuvEventLoop.LibuvLoopHandle);
        }

        public override void StartRead()
        {
            LibuvTcpHandle.ReadStart(AllocCallback, ReadCallback);
        }

        public override void StopRead()
        {
            LibuvTcpHandle.ReadStop();
        }

        public override void Write(ByteBuf byteBuf)
        {
            IUnmanagedByteBuf unmanagedByteBuf = byteBuf as IUnmanagedByteBuf;
            if (unmanagedByteBuf == null)
            {
                throw new InvalidOperationException();
            }

            IntPtr ptr;
            int len;
            unmanagedByteBuf.GetReadable(out ptr, out len);

            var buf = new LibuvNative.uv_buf_t(ptr, len, PlatformApis.IsWindows);

            // TODO: обрабатывать статус с ошибкой.
            int status = LibuvTcpHandle.TryWrite(buf);

            // Освобождаем буфер.
            byteBuf.Release();
        }

        private void AllocCallback(
            LibuvStreamHandle streamHandle,
            int suggestedsize,
            out LibuvNative.uv_buf_t buf)
        {
            // Тут мы можем просто взять поинтер, без буфера. Все равно поинтер не потеряется и будет передан
            // в ReadCallback, где будет завернут в буфер.
            int size;
            IntPtr dataPtr = _byteBufAllocator.GetDefaultDataIntPtr(out size);
            buf = new LibuvNative.uv_buf_t(dataPtr, size, PlatformApis.IsWindows);
        }

        private void ReadCallback(LibuvStreamHandle streamHandle, int status, ref LibuvNative.uv_buf_t buf)
        {
            if (status > 0)
            {
                IUnmanagedByteBuf byteBuf = _byteBufAllocator.WrapDefault(buf.Memory, buf.Len);
                byteBuf.SetWrite(status);

                OnRead((ByteBuf)byteBuf);
            }
            else if (status == 0)
            {
                // TODO:
                Console.WriteLine("ReadCallback. No data to read.");
            }
            else
            {
                string error = string.Format(
                    "Error #{0}. {1} {2}",
                    status,
                    Marshal.PtrToStringAnsi(LibuvNative.uv_err_name(status)),
                    Marshal.PtrToStringAnsi(LibuvNative.uv_strerror(status))
                );
                // TODO:
                Console.WriteLine("ReadCallback. {0}.", error);
            }
        }
    }
}