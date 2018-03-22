namespace VariantAnnotation.Interface.IO
{
    public interface IExtendedBinaryReader
    {
        bool ReadBoolean();
        byte ReadByte();
        string ReadString();
        uint ReadUInt32();
        string ReadAsciiString();
        ushort ReadOptUInt16();
        int ReadOptInt32();
        long ReadOptInt64();
    }
}