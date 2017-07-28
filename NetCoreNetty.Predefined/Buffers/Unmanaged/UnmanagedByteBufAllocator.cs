using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Predefined.Buffers.Unmanaged
{
    // TODO: Выделять один большой сегмент памяти и из него раздавать нужные сегменты
    public class UnmanagedByteBufAllocator : IUnmanagedByteBufAllocator
    {
        static private readonly int _memSegHeaderSize = MemorySegment.HeaderSize;
        static public readonly int DefaultBufSize;

        // TODO: реализовать вменяемый пулинг
        // Чтобы минимизировать кэшмисы в процессоре, нужно чтобы сегменты памяти были максимально близко друг к другу.
        private readonly ConcurrentQueue<IntPtr> _defaultMemorySegments = new ConcurrentQueue<IntPtr>();
        private readonly ConcurrentQueue<ByteBuf> _byteBufs = new ConcurrentQueue<ByteBuf>();

        static UnmanagedByteBufAllocator()
        {
            // TODO: поэкспериментировать
            DefaultBufSize = 192;
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

        internal void Release(IntPtr memSegPtr)
        {
            _defaultMemorySegments.Enqueue(memSegPtr);
        }
        
        internal void Release(UnmanagedByteBuf unmanagedByteBuf)
        {
            _byteBufs.Enqueue(unmanagedByteBuf);
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