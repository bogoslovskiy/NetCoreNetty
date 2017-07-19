using System;
using System.Runtime.CompilerServices;

namespace NetCoreNetty.Concurrency.Blocking
{
    public class BatchBlockingSwap2QueueT<T>
    {
        private readonly BatchBlockingQueue<T> _buffer1;
        private readonly BatchBlockingQueue<T> _buffer2;
        
        private BatchBlockingQueue<T> _writeBuffer;
        private BatchBlockingQueue<T> _readBuffer;

        private int _writeIteration;
        private int _readIteration;

        private bool _writeEntered;
        private bool _readEntered;

        public int Size { get; }

        public BatchBlockingSwap2QueueT(int size)
        {
            Size = size;
            _buffer1 = new BatchBlockingQueue<T>(size);
            _buffer2 = new BatchBlockingQueue<T>(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite()
        {
            if (!_writeEntered)
            {
                _writeBuffer = _writeIteration % 2 == 0
                    ? _buffer1
                    : _buffer2;
            
                _writeBuffer.EnterWrite();
                _writeEntered = true;
                return;
            }
            
            throw new InvalidOperationException("Already got write lock.");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead()
        {
            if (!_readEntered)
            {
                _readBuffer = _readIteration % 2 == 0
                    ? _buffer1
                    : _buffer2;
            
                _readBuffer.EnterRead();
                _readEntered = true;
                return;
            }
            
            throw new InvalidOperationException("Already got read lock.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite()
        {
            if (_writeEntered)
            {
                _writeBuffer.ExitWrite();
                _writeIteration++;
                _writeEntered = false;
                return;
            }
            
            throw new InvalidOperationException("Write lock already released.");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead()
        {
            if (_readEntered)
            {
                _readBuffer.ExitRead();
                _readIteration++;
                _readEntered = false;
                return;
            }
            
            throw new InvalidOperationException("Read lock already released.");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T data)
        {
            return _writeBuffer.TryEnqueue(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T data)
        {
            return _readBuffer.TryDequeue(out data);
        }
    }
}