using System;

namespace NetCoreNetty.Libuv
{
    public class LibuvLoopHandle : LibuvHandleBase
    {
        public void Init()
        {
            int loopSize = LibuvNative.uv_loop_size();
            InitUnmanaged(loopSize);
            LibuvNative.uv_loop_init(this);
        }

        public int RunDefault()
        {
            return LibuvNative.uv_run(this, 0 /* UV_RUN_DEFAULT */);
        }
        
        public int RunOnce()
        {
            return LibuvNative.uv_run(this, 1 /* UV_RUN_ONCE */);
        }

        public void Stop()
        {
            LibuvNative.uv_stop(this);
        }

        unsafe protected override bool ReleaseHandle()
        {
            IntPtr handlePtr = handle;
            if (handlePtr != IntPtr.Zero)
            {
                // uv_loop_close очищает gcHandlePtr.
                var gcHandlePtr = *(IntPtr*)handlePtr;

                LibuvNative.uv_loop_close(this.InternalGetHandle());

                handle = IntPtr.Zero;

                ReleaseUnmanaged(handlePtr, gcHandlePtr);
            }

            return true;
        }
    }
}