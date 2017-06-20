using System;
using System.Runtime.InteropServices;

namespace NetCoreNetty.Libuv
{
    // override ReleaseHandle оставим наследникам, чтобы они определяли, как освободиться.
    abstract public class LibuvHandleBase : SafeHandle
    {
        public override bool IsInvalid => handle == IntPtr.Zero;

        protected LibuvHandleBase()
            : base(IntPtr.Zero /* invalidHandleValue */, true /* ownsHandle */)
        {
        }

        unsafe static public THandle FromIntPtr<THandle>(IntPtr handle)
        {
            GCHandle gcHandle = GCHandle.FromIntPtr(*(IntPtr*)handle);
            return (THandle)gcHandle.Target;
        }

        internal IntPtr InternalGetHandle()
        {
            return handle;
        }

        unsafe protected void InitUnmanaged(int size)
        {
            handle = Marshal.AllocCoTaskMem(size);
            *(IntPtr*)handle = GCHandle.ToIntPtr(GCHandle.Alloc(this, GCHandleType.Weak));
        }

        unsafe static protected void ReleaseUnmanaged(IntPtr handlePtr)
        {
            var gcHandlePtr = *(IntPtr*)handlePtr;
            ReleaseUnmanaged(handlePtr, gcHandlePtr);
        }

        static protected void ReleaseUnmanaged(IntPtr handlePtr, IntPtr gcHandlePtr)
        {
            if (gcHandlePtr != IntPtr.Zero)
            {
                var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
                gcHandle.Free();
            }
            Marshal.FreeCoTaskMem(handlePtr);
        }
    }
}