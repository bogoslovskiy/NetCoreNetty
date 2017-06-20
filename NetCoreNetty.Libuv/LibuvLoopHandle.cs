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

        // TODO: mode = 0?
        public void Run(int mode)
        {
            LibuvNative.uv_run(this, mode);
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