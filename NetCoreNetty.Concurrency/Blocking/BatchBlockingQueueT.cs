using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCoreNetty.Concurrency.Blocking
{
    public class BatchBlockingQueue<T>
    {
        private readonly object _readLock = new object();
        private readonly object _writeLock = new object();

        private bool _readed;
        private bool _writed;
        
        private readonly T[] _buffer;
        private readonly int _size;
        private int _readIndex;
        private int _writeIndex;

        public BatchBlockingQueue(int size)
        {
            _size = size;
            _buffer = new T[size];
            _readed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite()
        {
            lock (_readLock)
            {
                while (!_readed)
                {
                    Monitor.Wait(_readLock);
                }

                // ****
                //Console.WriteLine("Entered to write.");
                // ****
                
                Monitor.Enter(_writeLock);
                
                // ****
                //Console.WriteLine("Got write lock.");
                // ****
                
                _readed = false;
                _writeIndex = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite()
        {
            if (!Monitor.IsEntered(_writeLock))
            {
                throw new InvalidOperationException();
            }

            _writed = true;
            
            Monitor.PulseAll(_writeLock);
            
            // ****
            //Console.WriteLine("Write pulse all.");
            // ****
            
            Monitor.Exit(_writeLock);
            
            // ****
            //Console.WriteLine("Write lock released.");
            // ****
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead()
        {
            lock (_writeLock)
            {
                while (!_writed)
                {
                    Monitor.Wait(_writeLock);
                }
                
                // ****
                //Console.WriteLine("Entered to read.");
                // ****

                Monitor.Enter(_readLock);
                
                // ****
                //Console.WriteLine("Got read lock.");
                // ****
                
                _writed = false;
                _readIndex = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead()
        {
            if (!Monitor.IsEntered(_readLock))
            {
                throw new InvalidOperationException();
            }

            _readed = true;
            
            Monitor.PulseAll(_readLock);
            
            // ****
            //Console.WriteLine("Read pulse all.");
            // ****
            
            Monitor.Exit(_readLock);
            
            // ****
            //Console.WriteLine("Read lock released.");
            // ****
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T data)
        {
            if (!Monitor.IsEntered(_writeLock))
            {
                throw new InvalidOperationException();
            }

            if (_writeIndex >= _size)
            {
                return false;
            }

            _buffer[_writeIndex] = data;
            
            // ****
            //Console.WriteLine($"Writed data {data}.");
            // ****
            
            _writeIndex++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T data)
        {
            if (!Monitor.IsEntered(_readLock))
            {
                throw new InvalidOperationException();
            }

            if (_readIndex >= _writeIndex)
            {
                data = default(T);
                return false;
            }

            data = _buffer[_readIndex];
            
            // ****
            //Console.WriteLine($"Readed data {data}.");
            // ****
            
            _buffer[_readIndex] = default(T);
            _readIndex++;
            return true;
        }
    }
}