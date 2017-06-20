using System;
using System.Runtime.InteropServices;

namespace NetCoreNetty.Libuv
{
    // TODO: throw if error для методов, которые возвращают статус.
    static public class LibuvNative
    {
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

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_loop_size();

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_handle_size(HandleType handleType);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_loop_init(LibuvLoopHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_loop_close(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_run(LibuvLoopHandle handle, int mode);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_stop(LibuvLoopHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_ref(LibuvHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_unref(LibuvHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern void uv_close(IntPtr handle, uv_close_cb close_cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_listen(LibuvStreamHandle handle, int backlog, uv_connection_cb cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_accept(LibuvStreamHandle server, LibuvStreamHandle client);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_read_start(LibuvStreamHandle handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_read_stop(LibuvStreamHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_try_write(LibuvStreamHandle handle, uv_buf_t[] bufs, int nbufs);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_tcp_init(LibuvLoopHandle loopHandle, LibuvTcpHandle tcpHandle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_tcp_bind(LibuvTcpHandle handle, ref SockAddr addr, int flags);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_ip4_addr(string ip, int port, out SockAddr addr);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern int uv_ip6_addr(string ip, int port, out SockAddr addr);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern IntPtr uv_err_name(int err);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        static public extern IntPtr uv_strerror(int err);

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






//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_fileno(UvHandle handle, ref IntPtr socket);
//
//
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_async_init(UvLoopHandle loop, UvAsyncHandle handle, Libuv.uv_async_cb cb);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public extern static int uv_async_send(UvAsyncHandle handle);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl, EntryPoint = "uv_async_send")]
//        public extern static int uv_unsafe_async_send(IntPtr handle);
//
//
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_tcp_open(UvTcpHandle handle, IntPtr hSocket);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_tcp_nodelay(UvTcpHandle handle, int enable);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_pipe_init(UvLoopHandle loop, UvPipeHandle handle, int ipc);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_pipe_bind(UvPipeHandle loop, string name);
//
//
//
//
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
//        public static extern void uv_pipe_connect(UvConnectRequest req, UvPipeHandle handle, string name, Libuv.uv_connect_cb cb);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public extern static int uv_pipe_pending_count(UvPipeHandle handle);
//
//
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        unsafe public static extern int uv_write(UvRequest req, UvStreamHandle handle, Libuv.uv_buf_t* bufs, int nbufs, Libuv.uv_write_cb cb);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        unsafe public static extern int uv_write2(UvRequest req, UvStreamHandle handle, Libuv.uv_buf_t* bufs, int nbufs, UvStreamHandle sendHandle, Libuv.uv_write_cb cb);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_shutdown(UvShutdownReq req, UvStreamHandle handle, Libuv.uv_shutdown_cb cb);
//

//
//
//
//
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_req_size(Libuv.RequestType reqType);
//
//
//
//
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_tcp_getsockname(UvTcpHandle handle, out SockAddr name, ref int namelen);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_tcp_getpeername(UvTcpHandle handle, out SockAddr name, ref int namelen);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int uv_walk(UvLoopHandle loop, Libuv.uv_walk_cb walk_cb, IntPtr arg);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        unsafe public static extern int uv_timer_init(UvLoopHandle loop, UvTimerHandle handle);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        unsafe public static extern int uv_timer_start(UvTimerHandle handle, Libuv.uv_timer_cb cb, long timeout, long repeat);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        unsafe public static extern int uv_timer_stop(UvTimerHandle handle);
//
//        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
//        unsafe public static extern long uv_now(UvLoopHandle loop);
//
//        [DllImport("WS2_32.dll", CallingConvention = CallingConvention.Winapi)]
//        unsafe public static extern int WSAIoctl(
//            IntPtr socket,
//            int dwIoControlCode,
//            int* lpvInBuffer,
//            uint cbInBuffer,
//            int* lpvOutBuffer,
//            int cbOutBuffer,
//            out uint lpcbBytesReturned,
//            IntPtr lpOverlapped,
//            IntPtr lpCompletionRoutine
//        );
//
//        [DllImport("WS2_32.dll", CallingConvention = CallingConvention.Winapi)]
//        public static extern int WSAGetLastError();
    }
}