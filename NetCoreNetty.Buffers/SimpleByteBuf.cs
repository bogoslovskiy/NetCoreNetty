using System;

namespace NetCoreNetty.Buffers
{
    public class SimpleByteBuf : ByteBuf
    {
        private readonly byte[] _bytes;
        private readonly int _minIndex;
        private readonly int _maxIndex;
        private int _currentReadIndex;
        private int _currentWriteIndex;

        public SimpleByteBuf(byte[] bytes)
        {
            _bytes = bytes;
            _minIndex = 0;
            _maxIndex = bytes.Length - 1;
            _currentReadIndex = _minIndex - 1;
            _currentWriteIndex = _minIndex - 1;
        }

        public SimpleByteBuf Concat(byte[] bytes)
        {
            byte[] newBytes = new byte[_bytes.Length + bytes.Length];
            Array.Copy(_bytes, 0, newBytes, 0, _bytes.Length);
            Array.Copy(bytes, 0, newBytes, _bytes.Length, bytes.Length);

            var newByteBuf = new SimpleByteBuf(newBytes);
            newByteBuf._currentReadIndex = this._currentReadIndex;

            return newByteBuf;
        }

        public override void Append(ByteBuf byteBuf)
        {
            throw new NotImplementedException();
        }

        public override void Release()
        {
            throw new NotImplementedException();
        }

        public override void ReleaseReaded()
        {
            throw new NotImplementedException();
        }

        public override int ReadableBytes()
        {
            return _maxIndex - _currentReadIndex;
        }

        public override void Back(int offset)
        {
            if (_minIndex > _currentReadIndex - offset)
            {
                throw new InvalidOperationException();
            }

            _currentReadIndex -= offset;
        }

        public override byte ReadByte()
        {
            if (_currentReadIndex + 1 > _maxIndex)
            {
                throw new InvalidOperationException();
            }

            _currentReadIndex++;
            return _bytes[_currentReadIndex];
        }

        public override short ReadShort()
        {
            throw new System.NotImplementedException();
        }

        public override ushort ReadUShort()
        {
            throw new System.NotImplementedException();
        }

        public override int ReadInt()
        {
            throw new System.NotImplementedException();
        }

        public override uint ReadUInt()
        {
            throw new System.NotImplementedException();
        }

        public override long ReadLong()
        {
            throw new System.NotImplementedException();
        }

        public override ulong ReadULong()
        {
            throw new System.NotImplementedException();
        }

        public override int ReadToOrRollback(byte stopByte, byte[] output, int startIndex, int len)
        {
            throw new System.NotImplementedException();
        }

        public override int ReadToOrRollback(byte stopByte1, byte stopByte2, byte[] output, int startIndex, int len)
        {
            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            int localCurrentIndex = _currentReadIndex;
            int readed = 0;

            while (localCurrentIndex < _maxIndex)
            {
                localCurrentIndex++;

                byte currentByte = _bytes[localCurrentIndex];

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
                _currentReadIndex = localCurrentIndex - 2;
                return readed;
            }

            return -1;
        }

        public override int SkipTo(byte stopByte, bool include)
        {
            throw new System.NotImplementedException();
        }

        public override int SkipTo(byte stopByte1, byte stopByte2, bool include)
        {
            int skipped = 0;

            bool stopByte1Matched = false;
            bool stopBytesMatched = false;

            int localCurrentIndex = _currentReadIndex;

            while (localCurrentIndex < _maxIndex)
            {
                localCurrentIndex++;
                skipped++;

                byte currentByte = _bytes[localCurrentIndex];

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
                    localCurrentIndex -= 2;
                }

                _currentReadIndex = localCurrentIndex;
                return skipped;
            }

            return -1;
        }

        public override int WritableBytes()
        {
            throw new NotImplementedException();
        }

        public int CopyTo(ByteBuf byteBuf, int maxLength)
        {
            // TODO: нормальная реализация

            var destinationByteBuf = (SimpleByteBuf) byteBuf;

            while (maxLength > 0)
            {
                _currentReadIndex++;
                destinationByteBuf._currentWriteIndex++;

                destinationByteBuf._bytes[destinationByteBuf._currentWriteIndex] =
                    _bytes[_currentReadIndex];

                maxLength--;
            }

            return maxLength;
        }

        public override void Write(byte @byte)
        {
            _currentWriteIndex++;
            _bytes[_currentWriteIndex] = @byte;
        }

        public override string Dump()
        {
            throw new NotImplementedException();
        }
    }
}