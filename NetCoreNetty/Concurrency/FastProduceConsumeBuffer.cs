using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCoreNetty.Concurrency
{
    public class FastProduceConsumeBuffer<TData>
    {
        private readonly int _size;
        
        // 0 - no data.
        // 1 - write.
        // 2 - writed.
        // 3 - read.
        private readonly long[] _lockBuffer;

        private readonly CircularBuffer<TData>[] _buffers;
        
        private int _readInterruptStatus;
        private int _writed;

        public FastProduceConsumeBuffer(int size)
        {
            _size = size;
            
            _lockBuffer = new long[_size];
            _buffers = new CircularBuffer<TData>[_size];
        }

        public void Write(CircularBuffer<TData> buffer, TData data)
        {
            var spinWait = new SpinWait();
            int idx;
            
            while (true)
            {
                idx = 0;
                bool gotLock = false;
                
                while (idx < _size)
                {
                    long @lock = Interlocked.CompareExchange(ref _lockBuffer[idx], 1, 0);
                    if (@lock == 0)
                    {
                        gotLock = true;
                        break;
                    }

                    idx++;
                }

                if (!gotLock)
                {
                    if (_writed < _size)
                    {
                        spinWait.Reset();
                        continue;
                    }

                    spinWait.SpinOnce();
                }
                else
                {
                    break;
                }
            }

            buffer.Write(data);
            _buffers[idx] = buffer;

            Interlocked.Increment(ref _writed);
            Interlocked.Exchange(ref _lockBuffer[idx], 2);
        }
        
        public void CompleteWriting()
        {
            Interlocked.Exchange(ref _readInterruptStatus, 1);
        }
        
        public void StartRead(Action<TData> callback)
        {
            var spinWait = new SpinWait();

            while (Interlocked.CompareExchange(ref _readInterruptStatus, 0, 0) == 0)
            {
                ReadCore(callback);
                
                if (_writed > 0)
                {
                    spinWait.Reset();
                    continue;
                }

                spinWait.SpinOnce();
            }
            
            ReadCore(callback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadCore(Action<TData> callback)
        {
            for (int i = 0; i < _size; i++)
            {
                long @lock = Interlocked.CompareExchange(ref _lockBuffer[i], 3, 2);
                if (@lock == 2)
                {
                    CircularBuffer<TData> circularBuffer = _buffers[i];
                    
                    bool read = circularBuffer.TryReadAll(callback);
                    if (read)
                    {
                        Interlocked.Decrement(ref _writed);
                        _buffers[i] = null;
                        Interlocked.Exchange(ref _lockBuffer[i], 0);
                    }
                    else
                    {
                        Interlocked.Exchange(ref _lockBuffer[i], 2);
                    }
                }
            }
        }
    }
}
