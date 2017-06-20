namespace NetCoreNetty.Utils.BitStrings
{
    public struct BitString64
    {
        private long _data;

        public void Set(int index, bool flag)
        {
            long mask = 1 << index;
            _data = flag ? _data | mask : _data & ~mask;
        }

        public bool Get(int index)
        {
            long mask = 1 << index;
            return (_data & mask) == mask;
        }

        public bool AllReseted()
        {
            return _data == default(long);
        }

        public BitString64 Clone()
        {
            var newBitArray = new BitString64();
            newBitArray._data = _data;
            return newBitArray;
        }
    }
}