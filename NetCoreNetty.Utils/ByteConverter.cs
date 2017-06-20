using System.Runtime.CompilerServices;

namespace NetCoreNetty.Utils
{
    static public class ByteConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public short GetShort(byte b1, byte b2)
        {
            short a = b1;
            a = (short)(a << 8 | b2);
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ushort GetUShort(byte b1, byte b2)
        {
            ushort a = b1;
            a = (ushort)(a << 8 | b2);
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int GetInt(byte b1, byte b2, byte b3, byte b4)
        {
            int a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public uint GetUInt(byte b1, byte b2, byte b3, byte b4)
        {
            uint a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public long GetLong(byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
        {
            long a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            a = a << 8 | b5;
            a = a << 8 | b6;
            a = a << 8 | b7;
            a = a << 8 | b8;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ulong GetULong(byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
        {
            ulong a = b1;
            a = a << 8 | b2;
            a = a << 8 | b3;
            a = a << 8 | b4;
            a = a << 8 | b5;
            a = a << 8 | b6;
            a = a << 8 | b7;
            a = a << 8 | b8;
            return a;
        }
    }
}