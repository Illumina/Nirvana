using System;

namespace IO
{
    public interface IBufferedBinaryReader : IDisposable
    {
        long BufferPosition { get; set; }
        void Reset();
        string ReadAsciiString();
        bool ReadBoolean();
        byte ReadByte();
        byte[] ReadBytes(int numBytes);
        void Read(byte[] buffer, int numBytes);
        double ReadDouble();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        int ReadOptInt32();
        long ReadOptInt64();
        ushort ReadOptUInt16();
        string ReadString();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
    }
}
