using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCoreNetty.Buffers.Unmanaged
{
    // TODO: проверить утечки, если стопнуть процесс dotnet - утечет ли память по выделенным сегментам.
    public class UnmanagedByteBufAllocator : IUnmanagedByteBufAllocator
    {
        static private readonly int _memSegHeaderSize = MemorySegment.HeaderSize;
        static public readonly int DefaultBufSize;

        // TODO: реализовать вменяемый пулинг
        private readonly ConcurrentQueue<IntPtr> _defaultMemorySegments = new ConcurrentQueue<IntPtr>();
        private readonly ConcurrentQueue<ByteBuf> _byteBufs = new ConcurrentQueue<ByteBuf>();

        static UnmanagedByteBufAllocator()
        {
            // TODO: поэкспериментировать
            DefaultBufSize = 256;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseDefaultPtr(IntPtr ptr)
        {
            _defaultMemorySegments.Enqueue(ptr);
        }

        public ByteBuf GetDefault()
        {
            int size;
            IntPtr dataPtr = GetDefaultDataIntPtr(out size);
            UnmanagedByteBuf byteBuf = (UnmanagedByteBuf)WrapDefault(dataPtr, 0 /* filledSize */);

            return byteBuf;
        }

        public IUnmanagedByteBuf GetUnmanaged(int size)
        {
            return (IUnmanagedByteBuf)Get(size);
        }

        public IUnmanagedByteBuf WrapDefault(IntPtr dataPtr, int filledSize)
        {
            IntPtr memSegPtr = MemorySegment.GetMemSegPtrByDataPtr(dataPtr);

            UnmanagedByteBuf byteBuf = GetByteBufCore();
            byteBuf.Attach(memSegPtr);
            byteBuf.SetWrite(filledSize);

            return byteBuf;
        }

        public IntPtr GetDefaultDataIntPtr(out int size)
        {
            size = DefaultBufSize - _memSegHeaderSize;
            IntPtr memSegPtr = GetDefaultIntPtrCore();
            return MemorySegment.GetDataPtr(memSegPtr);
        }

        public IUnmanagedByteBuf GetDefaultUnmanaged()
        {
            return (IUnmanagedByteBuf)GetDefault();
        }

        public ByteBuf Get(int size)
        {
            throw new NotImplementedException();
        }

        public void Release(ByteBuf buf)
        {
            // TODO: здесь утечка. буфер может быть кумулятивным. тогда надо освобождать все сегменты.
            IntPtr memSegPtr = ((UnmanagedByteBuf) buf).GetMemSegPtr();
            ReleaseDefaultPtr(memSegPtr);

            _byteBufs.Enqueue(buf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnmanagedByteBuf GetByteBufCore()
        {
            ByteBuf byteBuf;
            if (!_byteBufs.TryDequeue(out byteBuf))
            {
                byteBuf = new UnmanagedByteBuf(this);
            }

            return (UnmanagedByteBuf) byteBuf;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetDefaultIntPtrCore()
        {
            IntPtr intPtr;
            if (!_defaultMemorySegments.TryDequeue(out intPtr))
            {
                intPtr = AllocDefault();
            }

            MemorySegment.SetNext(intPtr, IntPtr.Zero);
            MemorySegment.SetPrev(intPtr, IntPtr.Zero);
            MemorySegment.SetSize(intPtr, DefaultBufSize - _memSegHeaderSize);
            MemorySegment.SetUsed(intPtr, DefaultBufSize - _memSegHeaderSize);

            return intPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr AllocDefault()
        {
            return Marshal.AllocCoTaskMem(DefaultBufSize);
        }
    }
}