using System;
using System.Runtime.CompilerServices;

namespace NetCoreNetty.Buffers.Unmanaged
{
    public class UnmanagedByteBuf : ByteBuf, IUnmanagedByteBuf
    {
        private struct State
        {
            public IntPtr MemSegPtr;
            public int MemSegSize;
            unsafe public byte* MemSegDataPtr;
            public int MemSegReadIndex;
            public int GlobalReaded;
            public int GlobalWrited;

            unsafe public State(
                IntPtr memSegPtr,
                int memSegSize,
                byte* memSegDataPtr,
                int memSegReadIndex,
                int globalReadIndex,
                int globalWriteIndex)
            {
                MemSegPtr = memSegPtr;
                MemSegSize = memSegSize;
                MemSegDataPtr = memSegDataPtr;
                MemSegReadIndex = memSegReadIndex;
                GlobalReaded = globalReadIndex;
                GlobalWrited = globalWriteIndex;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int RemainBytes()
            {
                return GlobalWrited - GlobalReaded;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Back(int offset)
            {
                MemSegReadIndex -= offset;
                GlobalReaded -= offset;

                if (MemSegReadIndex < -1)
                {
                    SwitchToPrev();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe public byte ReadByte()
            {
				if (MemSegReadIndex == MemSegSize - 1)
				{
					SwitchToNext();
				}
                
                MemSegReadIndex++;
                GlobalReaded++;

                return MemSegDataPtr[MemSegReadIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SwitchToNext()
            {
                IntPtr nextMemSegPtr = MemorySegment.GetNext(MemSegPtr);
                if (nextMemSegPtr != IntPtr.Zero)
                {
                    // Сместиться могли более чем на 1 слот, поэтому вычитаем весь предыдущий сегмент.
                    MemSegReadIndex -= MemSegSize;

                    SetCurrentMemSeg(nextMemSegPtr);
                    return;
                }

                throw new Exception();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SwitchToPrev()
            {
                IntPtr prevMemSegPtr = MemorySegment.GetPrev(MemSegPtr);
                if (prevMemSegPtr != IntPtr.Zero)
                {
                    SetCurrentMemSeg(prevMemSegPtr);

                    // Сместиться могли более чем на 1 слот, поэтому прибавляем весь текущий сегмент.
                    MemSegReadIndex += MemSegSize + 1;
                    return;
                }

                throw new Exception();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe private void SetCurrentMemSeg(IntPtr memSegPtr)
            {
                MemSegPtr = memSegPtr;
                MemSegSize = MemorySegment.GetUsed(MemSegPtr);
                MemSegDataPtr = (byte*) (void*) MemorySegment.GetDataPtr(MemSegPtr);
            }
        }

        private readonly IByteBufAllocator _allocator;

        private IntPtr _memSegPtr;
        private int _memSegSize;
        unsafe private byte* _memSegDataPtr;

        private IntPtr _lastMemSegPtr;

        private int _memSegReadIndex;
        private int _memSegWriteIndex;

        private int _globalReaded;
        private int _globalWrited;

        public UnmanagedByteBuf(IByteBufAllocator allocator)
        {
            _allocator = allocator;
        }

        // TODO: release прямо здесь
        public IntPtr GetMemSegPtr()
        {
            return _memSegPtr;
        }

        public void Attach(IntPtr memSeg)
        {
            _memSegPtr = memSeg;
            _lastMemSegPtr = memSeg;
            unsafe
            {
                _memSegDataPtr = (byte*) (void*) MemorySegment.GetDataPtr(memSeg);
            }
            _memSegSize = MemorySegment.GetUsed(memSeg);
            _memSegReadIndex = -1;
            _memSegWriteIndex = -1;
            _globalReaded = 0;
            _globalWrited = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetReadable(out IntPtr dataPtr, out int length)
        {
            // Учитывая то, кто использует этот метод, тут никогда не будет других чтений и никогда не будет больше
            // одного сегмента.
            length = _globalWrited;
            dataPtr = MemorySegment.GetDataPtr(_memSegPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWrite(int write)
        {
            _globalWrited = write;
        }

        // TODO: Проверки (например, что присоединяем буфер, который не начали читать).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Append(ByteBuf byteBuf)
        {
            UnmanagedByteBuf unmanagedByteBuf = (UnmanagedByteBuf) byteBuf;

            IntPtr appendixCurrentMemSegPtr = unmanagedByteBuf._memSegPtr;
            IntPtr appendixLastMemSegPtr = unmanagedByteBuf._lastMemSegPtr;

            MemorySegment.SetNext(_lastMemSegPtr, appendixCurrentMemSegPtr);
            MemorySegment.SetPrev(appendixCurrentMemSegPtr, _lastMemSegPtr);

            _lastMemSegPtr = appendixLastMemSegPtr;
            _globalWrited += unmanagedByteBuf._globalWrited;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Release()
        {
            _allocator.Release(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReleaseReaded()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadableBytes()
        {
            return _globalWrited - _globalReaded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Back(int offset)
        {
            _memSegReadIndex -= offset;
            _globalReaded -= offset;

            if (_memSegReadIndex < -1)
            {
                SwitchToPrev();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override byte ReadByte()
        {
			if (_memSegReadIndex == _memSegSize - 1)
			{
				SwitchToNext();
			}
            
            _memSegReadIndex++;
            _globalReaded++;

            return _memSegDataPtr[_memSegReadIndex];
        }

        public override short ReadShort()
        {
            throw new NotImplementedException();
        }

        public override ushort ReadUShort()
        {
            throw new NotImplementedException();
        }

        public override int ReadInt()
        {
            throw new NotImplementedException();
        }

        public override uint ReadUInt()
        {
            throw new NotImplementedException();
        }

        public override long ReadLong()
        {
            throw new NotImplementedException();
        }

        public override ulong ReadULong()
        {
            throw new NotImplementedException();
        }

        public override int ReadToOrRollback(byte stopByte, byte[] output, int startIndex, int len)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadToOrRollback(byte stopByte1, byte stopByte2, byte[] output, int startIndex, int len)
        {
            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            int readed = 0;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                byte currentByte = state.ReadByte();

                if (currentByte == stopByte2)
                {
                    if (stopByte1Matched)
                    {
                        stopBytesMatched = true;
                        break;
                    }
                }
                if (currentByte == stopByte1)
                {
                    stopByte1Matched = true;
                    continue;
                }

                stopByte1Matched = false;

                output[startIndex] = currentByte;
                readed++;
                startIndex++;

                if (startIndex == len)
                {
                    throw new Exception();
                }
            }

            if (stopBytesMatched)
            {
                state.Back(2);
                SetState(state);
                return readed;
            }

            return -1;
        }

        public override int SkipTo(byte stopByte, bool include)
        {
            throw new NotImplementedException();
        }

        public override int SkipTo(byte stopByte1, byte stopByte2, bool include)
        {
            int skipped = 0;

            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                skipped++;
                byte currentByte = state.ReadByte();

                if (currentByte == stopByte2)
                {
                    if (stopByte1Matched)
                    {
                        stopBytesMatched = true;
                        break;
                    }
                }
                if (currentByte == stopByte1)
                {
                    stopByte1Matched = true;
                    continue;
                }

                stopByte1Matched = false;
            }

            if (stopBytesMatched)
            {
                if (!include)
                {
                    skipped -= 2;
                    state.Back(2);
                }

                SetState(state);
                return skipped;
            }

            return -1;
        }

        public override int WritableBytes()
        {
            // TODO: Запись пока что невозможна в цепочку сегментов.
            return _memSegSize - _memSegWriteIndex - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public override void Write(byte @byte)
        {
			// TODO: Запись пока что невозможна в цепочку сегментов.
			if (_memSegWriteIndex == _memSegSize - 1)
			{
				throw new InvalidOperationException();
			}
            
            _memSegWriteIndex++;
            _globalWrited++;

            _memSegDataPtr[_memSegWriteIndex] = @byte;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private State GetState()
        {
            return new State(
                _memSegPtr,
                _memSegSize,
                _memSegDataPtr,
                _memSegReadIndex,
                _globalReaded,
                _globalWrited
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void SetState(State state)
        {
            _memSegPtr = state.MemSegPtr;
            _memSegSize = state.MemSegSize;
            _memSegDataPtr = state.MemSegDataPtr;
            _memSegReadIndex = state.MemSegReadIndex;
            _globalReaded = state.GlobalReaded;
            _globalWrited = state.GlobalWrited;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SwitchToNext()
        {
            IntPtr nextMemSegPtr = MemorySegment.GetNext(_memSegPtr);
            if (nextMemSegPtr != IntPtr.Zero)
            {
                // Сместиться могли более чем на 1 слот, поэтому вычитаем весь предыдущий сегмент.
                _memSegReadIndex -= _memSegSize;

                SetCurrentMemSeg(nextMemSegPtr);
                return;
            }

            throw new Exception();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SwitchToPrev()
        {
            IntPtr prevMemSegPtr = MemorySegment.GetPrev(_memSegPtr);
            if (prevMemSegPtr != IntPtr.Zero)
            {
                SetCurrentMemSeg(prevMemSegPtr);

                // Сместиться могли более чем на 1 слот, поэтому прибавляем весь текущий сегмент.
                _memSegReadIndex += _memSegSize + 1;
                return;
            }

            throw new Exception();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void SetCurrentMemSeg(IntPtr memSegPtr)
        {
            _memSegPtr = memSegPtr;
            _memSegSize = MemorySegment.GetUsed(_memSegPtr);
            _memSegDataPtr = (byte*) (void*) MemorySegment.GetDataPtr(_memSegPtr);
        }

		public override string Dump()
		{
			throw new NotImplementedException();
		}
    }
}