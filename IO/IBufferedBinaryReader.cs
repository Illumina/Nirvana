using System;

namespace IO
{
    public interface IBufferedBinaryReader : IDisposable
    {
        string ReadAsciiString();
        bool ReadBoolean();
        byte ReadByte();
        int ReadOptInt32();
        ushort ReadOptUInt16();
        uint ReadUInt32();
    }
}
