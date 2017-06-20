using System;

namespace NetCoreNetty.Libuv
{
    abstract public class LibuvStreamHandle : LibuvHandle
    {
        public delegate void ConnectionCallback(LibuvStreamHandle streamHandle, int status);

        public delegate void AllocCallback(
            LibuvStreamHandle streamHandle,
            int suggestedSize,
            out LibuvNative.uv_buf_t buf);

        public delegate void ReadCallback(LibuvStreamHandle streamHandle, int status, ref LibuvNative.uv_buf_t buf);

        [ThreadStatic]
        static private LibuvNative.uv_buf_t[] _writeBufs;

        private ConnectionCallback _connectionCallback;
        private AllocCallback _allocCallback;
        private ReadCallback _readCallback;

        public void Listen(int backlog, ConnectionCallback connectionCallback)
        {
            _connectionCallback = connectionCallback;
            LibuvNative.uv_listen(this, backlog, ConnectionCb);
        }

        public void Accept(LibuvStreamHandle clientStreamHandle)
        {
            LibuvNative.uv_accept(this /* serverStreamHandle */, clientStreamHandle);
        }

        public void ReadStart(AllocCallback allocCallback, ReadCallback readCallback)
        {
            _allocCallback = allocCallback;
            _readCallback = readCallback;
            LibuvNative.uv_read_start(this, AllocCb, ReadCb);
        }

        public void ReadStop()
        {
            LibuvNative.uv_read_stop(this);
        }

        public int TryWrite(LibuvNative.uv_buf_t buf)
        {
            _writeBufs = _writeBufs ?? new LibuvNative.uv_buf_t[1];
            _writeBufs[0] = buf;

            return LibuvNative.uv_try_write(this, _writeBufs, 1);
        }

        static private void ConnectionCb(IntPtr handle, int status)
        {
            LibuvStreamHandle streamHandle = FromIntPtr<LibuvStreamHandle>(handle);
            streamHandle._connectionCallback(streamHandle, status);
        }

        static private void AllocCb(IntPtr handle, int suggestedSize, out LibuvNative.uv_buf_t buf)
        {
            LibuvStreamHandle streamHandle = FromIntPtr<LibuvStreamHandle>(handle);
            streamHandle._allocCallback(streamHandle, suggestedSize, out buf);
        }

        static private void ReadCb(IntPtr handle, int status, ref LibuvNative.uv_buf_t buf)
        {
            LibuvStreamHandle streamHandle = FromIntPtr<LibuvStreamHandle>(handle);
            streamHandle._readCallback(streamHandle, status, ref buf);
        }
    }
}