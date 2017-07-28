using System;
using System.Runtime.CompilerServices;

namespace NetCoreNetty.Predefined.Buffers.Unmanaged
{
    static public class MemorySegment
    {
        // ptr - platform pointer size (4 or 8 bytes).
        // Size / Used - int values with size 4 bytes.
        //
        // + ptr      + ptr      + 4    + 4    + N                +
        // +----------+----------+------+------+------------------+
        // | Next     | Prev     | Size | Used | Data ........... |
        // +----------+----------+------+------+------------------+

        public const int HeaderSize = 24;

        static private readonly int OffsetPrev = IntPtr.Size;
        static private readonly int OffsetSize = 2 * IntPtr.Size;
        static private readonly int OffsetUsed = 2 * IntPtr.Size + 4;
        static private readonly int OffsetData = 2 * IntPtr.Size + 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public IntPtr GetNext(IntPtr memorySegmentPtr)
        {
            long nextAddress = *(long*) (void*) memorySegmentPtr;
            return new IntPtr(nextAddress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public void SetNext(IntPtr memorySegmentPtr, IntPtr nextMemorySegmentPtr)
        {
            *(long*) (void*) memorySegmentPtr = (long) nextMemorySegmentPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public IntPtr GetPrev(IntPtr memorySegmentPtr)
        {
            long prevAddress = *(long*)(void*)(memorySegmentPtr + OffsetPrev);
            return new IntPtr(prevAddress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public void SetPrev(IntPtr memorySegmentPtr, IntPtr prevMemorySegmentPtr)
        {
            *(long*) (void*) (memorySegmentPtr + OffsetPrev) = (long) prevMemorySegmentPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public int GetSize(IntPtr memorySegmentPtr)
        {
            return *(int*) (void*) (memorySegmentPtr + OffsetSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public void SetSize(IntPtr memorySegmentPtr, int size)
        {
            *(int*) (void*) (memorySegmentPtr + OffsetSize) = size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public int GetUsed(IntPtr memorySegmentPtr)
        {
            return *(int*) (void*) (memorySegmentPtr + OffsetUsed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static public void SetUsed(IntPtr memorySegmentPtr, int size)
        {
            *(int*) (void*) (memorySegmentPtr + OffsetUsed) = size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public IntPtr GetDataPtr(IntPtr memorySegmentPtr)
        {
            return memorySegmentPtr + OffsetData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public IntPtr GetMemSegPtrByDataPtr(IntPtr dataPtr)
        {
            return dataPtr - OffsetData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public IntPtr GetLast(IntPtr memorySegmentPtr)
        {
            IntPtr currentMemSeg;
            IntPtr nextMemSeg = memorySegmentPtr;
            do
            {
                currentMemSeg = nextMemSeg;
                nextMemSeg = GetNext(currentMemSeg);
            }
            while (nextMemSeg != IntPtr.Zero);

            return currentMemSeg;
        }
    }
}