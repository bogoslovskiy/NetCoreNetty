using System;
using System.Runtime.InteropServices;
using NetCoreNetty.Buffers;
using NetCoreNetty.Buffers.Unmanaged;
using NetCoreNetty.Core;
using NetCoreNetty.Libuv;

namespace NetCoreNetty.Channels
{
    public class LibuvTcpServerChannel : LibuvTcpHandle, IChannel
    {
        private Delegates.ChannelReadCallback _readCallback;

        // TODO: привести в порядок (избавиться от лишних методов в интерфейсе
        private readonly IUnmanagedByteBufAllocator _byteBufAllocator;

        public IByteBufAllocator ByteBufAllocator => _byteBufAllocator;

        public LibuvTcpServerChannel(IUnmanagedByteBufAllocator byteBufAllocator)
        {
            _byteBufAllocator = byteBufAllocator;
        }

        public void StartRead(Delegates.ChannelReadCallback readCallback)
        {
            _readCallback = readCallback;
            ReadStart(AllocCb, ReadCb);
        }

        public void StopRead()
        {
            ReadStop();
        }

        public void Write(ByteBuf byteBuf)
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
            int status = TryWrite(buf);

            // Освобождаем буфер.
            byteBuf.Release();
        }

        private void AllocCb(
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

        private void ReadCb(LibuvStreamHandle streamHandle, int status, ref LibuvNative.uv_buf_t buf)
        {
            if (status > 0)
            {
                // TODO:
                Console.WriteLine("ReadCallback. {0} bytes to read.", status);
                
                IUnmanagedByteBuf byteBuf = _byteBufAllocator.WrapDefault(buf.Memory, buf.Len);
                byteBuf.SetWrite(status);

                _readCallback(this, (ByteBuf)byteBuf);
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