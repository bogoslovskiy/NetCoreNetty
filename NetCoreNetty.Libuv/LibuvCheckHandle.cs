namespace NetCoreNetty.Libuv
{
    public class LibuvCheckHandle : LibuvHandle
    {
        public void Init(LibuvLoopHandle loopHandle)
        {
            int checkHandleSize = LibuvNative.uv_handle_size(LibuvNative.HandleType.CHECK);

            InitUnmanaged(checkHandleSize);

            LibuvNative.uv_check_init(loopHandle, this);
        }
    }
}