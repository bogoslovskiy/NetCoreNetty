using System;
using System.Runtime.InteropServices;

namespace NetCoreNetty.Libuv
{
    // TODO: throw if error для методов, которые возвращают статус.
    static public class LibuvNative
    {
        private const string LibName = "libuv";
        
        public enum HandleType
        {
            Unknown = 0,
            ASYNC,
            CHECK,
            FS_EVENT,
            FS_POLL,
            HANDLE,
            IDLE,
            NAMED_PIPE,
            POLL,
            PREPARE,
            PROCESS,
            STREAM,
            TCP,
            TIMER,
            TTY,
            UDP,
            SIGNAL,
        }

        public enum RequestType
        {
            Unknown = 0,
            REQ,
            CONNECT,
            WRITE,
            SHUTDOWN,
            UDP_SEND,
            FS,
            WORK,
            GETADDRINFO,
            GETNAMEINFO,
        }

        public struct uv_buf_t
        {
            // this type represents a WSABUF struct on Windows
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms741542(v=vs.85).aspx
            // and an iovec struct on *nix
            // http://man7.org/linux/man-pages/man2/readv.2.html
            // because the order of the fields in these structs is different, the field
            // names in this type don't have meaningful symbolic names. instead, they are
            // assigned in the correct order by the constructor at runtime

            private readonly IntPtr _field0;
            private readonly IntPtr _field1;

            public IntPtr Memory => PlatformApis.IsWindows ? _field1 : _field0;

            public int Len => (int)(PlatformApis.IsWindows ? _field0 : _field1);

            public uv_buf_t(IntPtr memory, int len, bool IsWindows)
            {
                if (IsWindows)
                {
                    _field0 = (IntPtr)len;
                    _field1 = memory;
                }
                else
                {
                    _field0 = memory;
                    _field1 = (IntPtr)len;
                }
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_close_cb(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_connection_cb(IntPtr serverHandle, int status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_alloc_cb(IntPtr server, int suggested_size, out uv_buf_t buf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_read_cb(IntPtr server, int nread, ref uv_buf_t buf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_prepare_cb(IntPtr prepareHandle);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_check_cb(IntPtr checkHandle);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_async_cb(IntPtr asyncHandle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_loop_size();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_handle_size(HandleType handleType);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_loop_init(LibuvLoopHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_loop_close(IntPtr handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_run(LibuvLoopHandle handle, int mode);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_stop(LibuvLoopHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_ref(LibuvHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_unref(LibuvHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_close(IntPtr handle, uv_close_cb close_cb);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_listen(LibuvStreamHandle handle, int backlog, uv_connection_cb cb);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_accept(LibuvStreamHandle server, LibuvStreamHandle client);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_read_start(LibuvStreamHandle handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_read_stop(LibuvStreamHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_try_write(LibuvStreamHandle handle, uv_buf_t[] bufs, int nbufs);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_tcp_init(LibuvLoopHandle loopHandle, LibuvTcpHandle tcpHandle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_tcp_bind(LibuvTcpHandle handle, ref SockAddr addr, int flags);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_ip4_addr(string ip, int port, out SockAddr addr);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_ip6_addr(string ip, int port, out SockAddr addr);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern IntPtr uv_err_name(int err);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern IntPtr uv_strerror(int err);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_prepare_init(LibuvLoopHandle loopHandle, LibuvPrepareHandle prepareHandle);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_prepare_start(LibuvPrepareHandle prepareHandle, uv_prepare_cb prepare_cb);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_prepare_stop(LibuvPrepareHandle prepareHandle);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_check_init(LibuvLoopHandle loopHandle, LibuvCheckHandle checkHandle);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_check_start(LibuvCheckHandle checkHandle, uv_check_cb check_cb);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_check_stop(LibuvCheckHandle checkHandle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_async_init(
            LibuvLoopHandle loopHandle, 
            LibuvAsyncHandle asyncHandle,
            uv_async_cb async_cb);
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_async_send(LibuvAsyncHandle asyncHandle);
        
        
        // TODO:
//        static public void ThrowIfErrored(int statusCode)
//        {
//            // Note: method is explicitly small so the success case is easily inlined
//            if (statusCode < 0)
//            {
//                ThrowError(statusCode);
//            }
//        }
//
//        static private void ThrowError(int statusCode)
//        {
//            // Note: only has one throw block so it will marked as "Does not return" by the jit
//            // and not inlined into previous function, while also marking as a function
//            // that does not need cpu register prep to call (see: https://github.com/dotnet/coreclr/pull/6103)
//            throw GetError(statusCode);
//        }
//
//        public void Check(int statusCode, out Exception error)
//        {
//            // Note: method is explicitly small so the success case is easily inlined
//            error = statusCode < 0 ? GetError(statusCode) : null;
//        }
//
//        [MethodImpl(MethodImplOptions.NoInlining)]
//        static private UvException GetError(int statusCode)
//        {
//            // Note: method marked as NoInlining so it doesn't bloat either of the two preceeding functions
//            // Check and ThrowError and alter their jit heuristics.
//            var errorName = err_name(statusCode);
//            var errorDescription = strerror(statusCode);
//            return new UvException("Error " + statusCode + " " + errorName + " " + errorDescription, statusCode);
//        }

    }
}