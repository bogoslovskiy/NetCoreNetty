using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetCoreNetty.Libuv
{
    static public class PlatformApis
    {
        static PlatformApis()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsDarwin = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        static public bool IsWindows { get; }

        static public bool IsDarwin { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long VolatileRead(ref long value)
        {
            if (IntPtr.Size == 8)
            {
                return Volatile.Read(ref value);
            }
            else
            {
                // Avoid torn long reads on 32-bit
                return Interlocked.Read(ref value);
            }
        }
    }
}
