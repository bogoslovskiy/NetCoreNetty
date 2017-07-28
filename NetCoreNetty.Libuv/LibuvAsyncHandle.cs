using System.Runtime.CompilerServices;

namespace NetCoreNetty.Libuv
{
    public class LibuvAsyncHandle : LibuvHandle
    {
        public void Init(LibuvLoopHandle loopHandle, LibuvNative.uv_async_cb asyncCallback)
        {
            int handleSize = LibuvNative.uv_handle_size(LibuvNative.HandleType.ASYNC);

            InitUnmanaged(handleSize);

            LibuvNative.uv_async_init(loopHandle, this, asyncCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send()
        {
            LibuvNative.uv_async_send(this);
        }
    }
}