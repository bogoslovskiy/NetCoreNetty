namespace NetCoreNetty.Buffers
{
    abstract public class ByteBuf
    {
        abstract public bool Released { get; }
        
        abstract public string Dump();
        
        abstract public void Append(ByteBuf byteBuf);

        abstract public void Release();

        abstract public void ReleaseReaded();

        abstract public int ReadableBytes();

        abstract public void Back(int offset);

        abstract public byte ReadByte();

        abstract public short ReadShort();

        abstract public ushort ReadUShort();

        abstract public int ReadInt();

        abstract public uint ReadUInt();

        abstract public long ReadLong();

        abstract public ulong ReadULong();

        abstract public int ReadToOrRollback(
            byte stopByte,
            byte[] output,
            int startIndex,
            int len);

        abstract public int ReadToOrRollback(
            byte stopByte1,
            byte stopByte2,
            byte[] output,
            int startIndex,
            int len);

        abstract public int SkipTo(byte stopByte, bool include);

        abstract public int SkipTo(byte stopByte1, byte stopByte2, bool include);

        abstract public int WritableBytes();
        
        abstract public void Write(byte @byte);

        // TODO: write*
    }
}