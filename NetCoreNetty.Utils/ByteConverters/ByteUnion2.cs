using System.Runtime.InteropServices;

namespace NetCoreNetty.Utils.ByteConverters
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteUnion2
    {
        [FieldOffset(0)]
        public short Short;

        [FieldOffset(0)]
        public ushort UShort;

        [FieldOffset(0)]
        public char Char;

        [FieldOffset(0)]
        public byte B1;

        [FieldOffset(1)]
        public byte B2;
    }
}