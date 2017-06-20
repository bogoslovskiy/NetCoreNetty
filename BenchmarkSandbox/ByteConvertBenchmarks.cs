using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace BenchmarkSandbox
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteMagic2
    {
        [FieldOffset(0)]
        public ushort UShort;

        [FieldOffset(0)]
        public byte H;

        [FieldOffset(1)]
        public byte L;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ByteMagic4
    {
        [FieldOffset(0)]
        public uint UInt;

        [FieldOffset(0)]
        public byte B1;

        [FieldOffset(1)]
        public byte B2;

        [FieldOffset(2)]
        public byte B3;

        [FieldOffset(3)]
        public byte B4;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ByteMagic8
    {
        [FieldOffset(0)]
        public ulong ULong;

        [FieldOffset(0)]
        public byte B1;

        [FieldOffset(1)]
        public byte B2;

        [FieldOffset(2)]
        public byte B3;

        [FieldOffset(3)]
        public byte B4;

        [FieldOffset(4)]
        public byte B5;

        [FieldOffset(5)]
        public byte B6;

        [FieldOffset(6)]
        public byte B7;

        [FieldOffset(7)]
        public byte B8;
    }


    static public class BitByteMagic
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static public uint GetUInt(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            uint a = byte1;
            a = a << 8 | byte2;
            a = a << 8 | byte3;
            a = a << 8 | byte4;
            return a;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static public ushort GetUShort(byte byte1, byte byte2)
        {
            ushort a = byte1;
            a = (ushort)(a << 8 | byte2);
            return a;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static public ulong GetULong(byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7, byte byte8)
        {
            ulong a = byte1;
            a = a << 8 | byte2;
            a = a << 8 | byte3;
            a = a << 8 | byte4;
            a = a << 8 | byte5;
            a = a << 8 | byte6;
            a = a << 8 | byte7;
            a = a << 8 | byte8;
            return a;
        }
    }

    static public class ClrByteMagic
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static public ushort GetUShort(byte byte1, byte byte2)
        {
            ByteMagic2 bm = new ByteMagic2();
            bm.H = byte2;
            bm.L = byte1;

            return bm.UShort;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static public uint GetUInt(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            ByteMagic4 bm = new ByteMagic4();
            bm.B1 = byte4;
            bm.B2 = byte3;
            bm.B3 = byte2;
            bm.B4 = byte1;

            return bm.UInt;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static public ulong GetULong(byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7, byte byte8)
        {
            var bm = new ByteMagic8();
            bm.B1 = byte8;
            bm.B2 = byte7;
            bm.B3 = byte6;
            bm.B4 = byte5;
            bm.B5 = byte4;
            bm.B6 = byte3;
            bm.B7 = byte2;
            bm.B8 = byte1;

            return bm.ULong;
        }
    }

    static public class UnsafeByteMagic
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        unsafe static public ushort GetUShort(byte byte1, byte byte2)
        {
            ushort result;
            byte* bytesPtr = (byte*)&result;
            bytesPtr[0] = byte2;
            bytesPtr[1] = byte1;

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        unsafe static public uint GetUInt(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            uint result;
            byte* bytesPtr = (byte*)&result;
            bytesPtr[0] = byte4;
            bytesPtr[1] = byte3;
            bytesPtr[2] = byte2;
            bytesPtr[3] = byte1;

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        unsafe static public ulong GetULong(byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7, byte byte8)
        {
            ulong result;
            byte* bytesPtr = (byte*)&result;
            bytesPtr[0] = byte8;
            bytesPtr[1] = byte7;
            bytesPtr[2] = byte6;
            bytesPtr[3] = byte5;
            bytesPtr[4] = byte4;
            bytesPtr[5] = byte3;
            bytesPtr[6] = byte2;
            bytesPtr[7] = byte1;

            return result;
        }
    }

    [CoreJob]
    public class ByteConvertBench
    {
        private byte byte1;
        private byte byte2;
        private byte byte3;
        private byte byte4;
        private byte byte5;
        private byte byte6;
        private byte byte7;
        private byte byte8;

        public ByteConvertBench()
        {
            var rnd = new Random();
            byte1 = (byte)rnd.Next(255);
            byte2 = (byte)rnd.Next(255);
            byte3 = (byte)rnd.Next(255);
            byte4 = (byte)rnd.Next(255);
            byte5 = (byte)rnd.Next(255);
            byte6 = (byte)rnd.Next(255);
            byte7 = (byte)rnd.Next(255);
            byte8 = (byte)rnd.Next(255);
        }

        [Benchmark]
        public uint ClrBytes2()
        {
            return ClrByteMagic.GetUShort(byte1, byte2);
        }

        [Benchmark]
        public uint UnsafeBytes2()
        {
            return UnsafeByteMagic.GetUShort(byte1, byte2);
        }
        [Benchmark]
        public uint BitBytesSergey2()
        {
            return BitByteMagic.GetUShort(byte1, byte2);
        }

        [Benchmark]
        public uint ClrBytes4()
        {
            return ClrByteMagic.GetUInt(byte1, byte2, byte3, byte4);
        }

        [Benchmark]
        public uint UnsafeBytes4()
        {
            return UnsafeByteMagic.GetUInt(byte1, byte2, byte3, byte4);
        }
        [Benchmark]
        public uint BitBytesSergey4()
        {
            return BitByteMagic.GetUInt(byte1, byte2, byte3, byte4);
        }

        [Benchmark]
        public ulong ClrBytes8()
        {
            return ClrByteMagic.GetULong(byte1, byte2, byte3, byte4, byte5, byte6, byte7, byte8);
        }

        [Benchmark]
        public ulong UnsafeBytes8()
        {
            return UnsafeByteMagic.GetULong(byte1, byte2, byte3, byte4, byte5, byte6, byte7, byte8);
        }

        [Benchmark]
        public ulong BitBytesSergey8()
        {
            return BitByteMagic.GetULong(byte1, byte2, byte3, byte4, byte5, byte6, byte7, byte8);
        }
    }
}