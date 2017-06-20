using System;

namespace NetCoreNetty.Libuv
{
    abstract public class LibuvHandle : LibuvHandleBase
    {
        public void Reference()
        {
            LibuvNative.uv_ref(this);
        }

        public void Unreference()
        {
            LibuvNative.uv_unref(this);
        }

        // TODO: проверка на поток
        protected override bool ReleaseHandle()
        {
            IntPtr handlePtr = handle;
            if (handlePtr != IntPtr.Zero)
            {
                handle = IntPtr.Zero;

                LibuvNative.uv_close(handlePtr, ReleaseUnmanaged);
            }
            return true;
        }
    }
}