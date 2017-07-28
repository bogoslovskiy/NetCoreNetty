using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCoreNetty.Concurrency
{
    public class CircularBuffer<TData>
    {
        private readonly int _size;
        private readonly TData[] _dataBuffer;
        // 0 - contains no data.
        // 1 - contains data.
        private readonly long[] _dataStateBuffer;

        private int _writeIndex = 1;
        private int _readIndex;

        // 0 - no lock.
        // 1 - read lock.
        private int _readLock;

        public CircularBuffer(int size)
        {
            _size = size;
            _dataBuffer = new TData[_size];
            _dataStateBuffer = new long[_size];
            // 1 - contains data.
            _dataStateBuffer[0] = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(TData data)
        {
            long dataState = Interlocked.Read(ref _dataStateBuffer[_writeIndex]);
            if (dataState != 0)
            {
                var spinWait = new SpinWait();

                while (dataState != 0)
                {
                    spinWait.SpinOnce();
                    dataState = Interlocked.Read(ref _dataStateBuffer[_writeIndex]);
                }
            }
            
            _dataBuffer[_writeIndex] = data;
            
            Interlocked.Exchange(ref _dataStateBuffer[_writeIndex], 1);
            
            SetNextWriteIndex();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadAll(Action<TData> callback)
        {
            int readLock = Interlocked.CompareExchange(ref _readLock, 1, 0);
            if (readLock == 0)
            {
                ReadAll(callback);

                Interlocked.Exchange(ref _readLock, 0);
                
                return true;
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadAll(Action<TData> callback)
        {
            int nextReadIndex = GetNextReadIndex();
            
            long dataState = Interlocked.Read(ref _dataStateBuffer[nextReadIndex]);
            while (dataState == 1)
            {
                Interlocked.Exchange(ref _dataStateBuffer[_readIndex], 0);
                
                TData data = _dataBuffer[nextReadIndex];
                _dataBuffer[nextReadIndex] = default(TData);
                callback(data);

                _readIndex = nextReadIndex;

                nextReadIndex = GetNextReadIndex();
                dataState = Interlocked.Read(ref _dataStateBuffer[nextReadIndex]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetNextWriteIndex()
        {
            if (_writeIndex == _size - 1)
            {
                _writeIndex = 0;
            }
            else
            {
                _writeIndex++;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNextReadIndex()
        {
            if (_readIndex == _size - 1)
            {
                return 0;
            }
            
            return _readIndex + 1;
        }
    }
}