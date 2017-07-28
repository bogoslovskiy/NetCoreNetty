using System.Runtime.InteropServices;

namespace NetCoreNetty.Utils.ByteConverters
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteUnion4
    {
        [FieldOffset(0)]
        public int Int;

        [FieldOffset(0)]
        public uint UInt;

        [FieldOffset(0)]
        public float Float;

        [FieldOffset(0)]
        public byte B1;

        [FieldOffset(1)]
        public byte B2;

        [FieldOffset(2)]
        public byte B3;

        [FieldOffset(3)]
        public byte B4;
    }
}