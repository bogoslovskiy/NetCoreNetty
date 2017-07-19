namespace NetCoreNetty.Libuv
{
    public class LibuvPrepareHandle : LibuvHandle
    {
        public void Init(LibuvLoopHandle loopHandle)
        {
            int prepareHandleSize = LibuvNative.uv_handle_size(LibuvNative.HandleType.PREPARE);

            InitUnmanaged(prepareHandleSize);

            LibuvNative.uv_prepare_init(loopHandle, this);
        }

        public void Start(LibuvNative.uv_prepare_cb prepareCallback)
        {
            LibuvNative.uv_prepare_start(this, prepareCallback);
        }

        public void Stop()
        {
            LibuvNative.uv_prepare_stop(this);
        }
    }
}