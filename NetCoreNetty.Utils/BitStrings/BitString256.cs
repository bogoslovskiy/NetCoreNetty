using System;

namespace NetCoreNetty.Utils.BitStrings
{
    public struct BitString256
    {
        private long _bits0;
        private long _bits1;
        private long _bits2;
        private long _bits3;

        static private readonly long[] Masks;

        static BitString256()
        {
            Masks = new long[256];
            for (int index = 0; index < 256; index++)
            {
                if (index < 64)
                {
                    Masks[index] = 1 << index;
                }
                else if (index < 128)
                {
                    Masks[index] = 1 << (index - 64);
                }
                else if (index < 192)
                {
                    Masks[index] = 1 << (index - 128);
                }
                else if (index < 256)
                {
                    Masks[index] = 1 << (index - 192);
                }
            }
        }

        public void Set(int index)
        {
            if (index < 64)
            {
                _bits0 = _bits0 | 1 << index;
            }
            else if (index < 128)
            {
                _bits1 = _bits1 | Masks[index];
            }
            else if (index < 192)
            {
                _bits2 = _bits2 | Masks[index];
            }
            else if (index < 256)
            {
                _bits3 = _bits3 | Masks[index];
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void Reset(int index)
        {
            if (index < 64)
            {
                _bits0 = _bits0 & ~(1 << index);
            }
            else if (index < 128)
            {
                _bits1 = _bits1 & ~Masks[index];
            }
            else if (index < 192)
            {
                _bits2 = _bits2 & ~Masks[index];
            }
            else if (index < 256)
            {
                _bits3 = _bits3 & ~Masks[index];
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public bool Get(int index)
        {
            if (index < 64)
            {
                long mask = 1 << index;
                return (_bits0 & mask) == mask;
            }
            if (index < 128)
            {
                long mask = Masks[index];
                return (_bits1 & mask) == mask;
            }
            if (index < 192)
            {
                long mask = Masks[index];
                return (_bits2 & mask) == mask;
            }
            if (index < 256)
            {
                long mask = Masks[index];
                return (_bits3 & mask) == mask;
            }
            throw new IndexOutOfRangeException();
        }
    }
}