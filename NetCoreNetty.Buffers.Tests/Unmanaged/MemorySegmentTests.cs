using System;
using System.Runtime.InteropServices;
using NetCoreNetty.Buffers.Unmanaged;
using Xunit;

namespace NetCoreNetty.Buffers.Tests.Unmanaged
{
    public class MemorySegmentTests
    {
        [Fact]
        public void TestHeader()
        {
            IntPtr ptrFirst = IntPtr.Zero;
            IntPtr ptrSecond = IntPtr.Zero;

            try
            {
                Assert.True(IntPtr.Size == 8);

                ptrFirst = Alloc(10);
                ptrSecond = Alloc(10);

                MemorySegment.SetNext(ptrFirst, IntPtr.Zero);
                MemorySegment.SetPrev(ptrSecond, IntPtr.Zero);

                MemorySegment.SetSize(ptrFirst, 0);
                MemorySegment.SetSize(ptrSecond, 0);

                MemorySegment.SetUsed(ptrFirst, 0);
                MemorySegment.SetUsed(ptrSecond, 0);

                Assert.True(MemorySegment.GetNext(ptrFirst) == IntPtr.Zero);
                Assert.True(MemorySegment.GetPrev(ptrSecond) == IntPtr.Zero);

                Assert.True(MemorySegment.GetSize(ptrFirst) == 0);
                Assert.True(MemorySegment.GetSize(ptrSecond) == 0);

                Assert.True(MemorySegment.GetUsed(ptrFirst) == 0);
                Assert.True(MemorySegment.GetUsed(ptrSecond) == 0);

                MemorySegment.SetNext(ptrFirst, ptrSecond);
                MemorySegment.SetPrev(ptrSecond, ptrFirst);

                MemorySegment.SetSize(ptrFirst, 10);
                MemorySegment.SetSize(ptrSecond, 9);

                MemorySegment.SetUsed(ptrFirst, 8);
                MemorySegment.SetUsed(ptrSecond, 4);

                Assert.True(MemorySegment.GetNext(ptrFirst) == ptrSecond);
                Assert.True(MemorySegment.GetPrev(ptrSecond) == ptrFirst);

                Assert.True(MemorySegment.GetSize(ptrFirst) == 10);
                Assert.True(MemorySegment.GetSize(ptrSecond) == 9);

                Assert.True(MemorySegment.GetUsed(ptrFirst) == 8);
                Assert.True(MemorySegment.GetUsed(ptrSecond) == 4);

                Assert.True(
                    MemorySegment.GetUsed(ptrFirst) ==
                    MemorySegment.GetUsed(
                        MemorySegment.GetMemSegPtrByDataPtr(
                            MemorySegment.GetDataPtr(ptrFirst)
                        )
                    )
                );
            }
            finally
            {
                Release(ptrFirst);
                Release(ptrSecond);
            }
        }

        private IntPtr Alloc(int dataSize)
        {
            return Marshal.AllocCoTaskMem(dataSize + MemorySegment.HeaderSize);
        }

        private void Release(IntPtr ptr)
        {
            Marshal.FreeCoTaskMem(ptr);
        }
    }
}