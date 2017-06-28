using System;
using System.Runtime.CompilerServices;
using NetCoreNetty.Utils;

namespace NetCoreNetty.Buffers.Unmanaged
{
    // TODO: добавить и прописать нормальные типы исключений
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

        // TODO: заменить на интерфейс? 
        private readonly UnmanagedByteBufAllocator _allocator;

        private IntPtr _memSegPtr;
        private int _memSegSize;
        unsafe private byte* _memSegDataPtr;

        private IntPtr _lastMemSegPtr;

        private int _memSegReadIndex;
        private int _memSegWriteIndex;

        private int _globalReaded;
        private int _globalWrited;

        private bool _released;

        public override bool Released => _released;
        
        public UnmanagedByteBuf(UnmanagedByteBufAllocator allocator)
        {
            _allocator = allocator;
        }

        ~UnmanagedByteBuf()
        {
            // Пул объектов такого типа должен быть устроен таким образом,
            // чтобы не хранить ссылку на отданный объект.
            // Таким образом, если текущий объект забыли отдать в пул, не должно быть
            // ссылок на него, чтобы сборщик его почистил, а при финализации объект мог
            // отдать неуправляемый ресурс обратно в пул.
            
            // Если "какой-то" сторонний объект не отдал данный объект в пул удерживает ссылку на него, то
            // мы не можем контролировать такие утечки, они полностью на совести "стороннего" кода.
            
            // Возвращаем куски памяти из неуправляемой кучи в пул.
            ReleaseMemorySegments();
            
            // Сам объект в пул вернуть не можем, т.к. он уничтожается сборщиком.
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
            _released = false;
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
            
            // Т.к. мы все забрали у присоединяемого буфера, то буфер как обертка больше не нужен.
            // Освобождаем его.
            unmanagedByteBuf.ReleaseCore();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Release()
        {
            // Разбор (можно называть это деконструкцией) объекта и возврат в пул его составляющих осуществляет
            // сам объект. Пул должен принимать только те куски на возврат, что ему отдают.
            
            // Возвращаем куски памяти из неуправляемой кучи в пул.
            ReleaseMemorySegments();

            ReleaseCore();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReleaseReaded()
        {
            // Вычисляем, начиная с какого сегмента можно освободить прочитанную цепочку сегментов.
            // Тут возможны 2 варианта:
            // - Весь буфер уже прочитан, тогда можно освободить все.
            // - Буфер прочитан не полностью, тогда можно освободить все предыдущие, а текущий оставить.
            
            // Сценарий с освобождением всей цепочки требует более тщательной реализации и пока невозможен,
            // поэтому всегда будем освобождать только начиная с предыдущего.
            // Дело в том, что у буфера пока не может не быть какого-то сегмента, а при освобождении всех именно так
            // и получится, но сам буфер может использоваться для аккумуляции дальше. Будет реализовано потом, если
            // потребуется.

            IntPtr memSegPtr = MemorySegment.GetPrev(_memSegPtr);
            
            // Отвязываем от текущего.
            MemorySegment.SetPrev(_memSegPtr, IntPtr.Zero);
            
            ReleaseMemorySegmentsAt(memSegPtr);
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
                // TODO: могли откатиться на несколько сегментов назад
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override short ReadShort()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();

            return ByteConverter.GetShort(b1, b2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ushort ReadUShort()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();

            return ByteConverter.GetUShort(b1, b2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadInt()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();

            return ByteConverter.GetInt(b1, b2, b3, b4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadUInt()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();

            return ByteConverter.GetUInt(b1, b2, b3, b4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long ReadLong()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();
            byte b5 = ReadByte();
            byte b6 = ReadByte();
            byte b7 = ReadByte();
            byte b8 = ReadByte();

            return ByteConverter.GetLong(b1, b2, b3, b4, b5, b6, b7, b8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong ReadULong()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();
            byte b5 = ReadByte();
            byte b6 = ReadByte();
            byte b7 = ReadByte();
            byte b8 = ReadByte();

            return ByteConverter.GetULong(b1, b2, b3, b4, b5, b6, b7, b8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadToOrRollback(byte stopByte, byte[] output, int startIndex, int len)
        {
            bool stopByteMatched = false;

            int readed = 0;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                byte currentByte = state.ReadByte();

                if (currentByte == stopByte)
                {
                    stopByteMatched = true;
                    break;
                }

                output[startIndex] = currentByte;
                readed++;
                startIndex++;

                if (startIndex == len)
                {
                    throw new Exception();
                }
            }

            if (stopByteMatched)
            {
                state.Back(1);
                SetState(state);
                return readed;
            }

            return -1;
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
            int skipped = 0;

            bool stopByteMatched = false;

            State state = GetState();

            while (state.RemainBytes() > 0)
            {
                skipped++;
                byte currentByte = state.ReadByte();

                if (currentByte == stopByte)
                {
                    stopByteMatched = true;
                    break;
                }
            }

            if (stopByteMatched)
            {
                if (!include)
                {
                    skipped -= 1;
                    state.Back(1);
                }

                SetState(state);
                return skipped;
            }

            return -1;
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseCore()
        {
            // Инициализируем поля пустыми значениями.
            Clear();
            
            // Теперь возвращем в пул сам объект.
            _allocator.Release(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseMemorySegments()
        {
            // Для освобождения всей цепочки сегментов памяти указываем стартовый сегмент - последний.
            ReleaseMemorySegmentsAt(_lastMemSegPtr);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseMemorySegmentsAt(IntPtr memSegPtr)
        {
            // Проходим от указанного сегмента по связям с предыдущими.
            while (memSegPtr != IntPtr.Zero)
            {
                IntPtr releaseMemSegPtr = memSegPtr;
                memSegPtr = MemorySegment.GetPrev(releaseMemSegPtr);
                
                // Чистим связи с другими сегментами.
                MemorySegment.SetPrev(releaseMemSegPtr, IntPtr.Zero);
                MemorySegment.SetNext(releaseMemSegPtr, IntPtr.Zero);
                
                _allocator.Release(releaseMemSegPtr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Clear()
        {
            _memSegPtr = IntPtr.Zero;
            _lastMemSegPtr = IntPtr.Zero;
            unsafe
            {
                _memSegDataPtr = (byte*) (void*) IntPtr.Zero;;
            }
            _memSegSize = 0;
            _memSegReadIndex = -1;
            _memSegWriteIndex = -1;
            _globalReaded = 0;
            _globalWrited = 0;
            
            // Обязательно устанавливаем флаг, указывающий на то, что буфер больше нельзя использовать.
            _released = true;
        }

        public override string Dump()
        {
            throw new NotImplementedException();
        }
    }
}