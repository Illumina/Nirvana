namespace VariantAnnotation.Interface
{
    public interface IExtendedBinaryWriter
    {
        void Write(bool b);
        void Write(byte b);
        void Write(ushort us);
        void WriteOpt(int value);
        void WriteOptAscii(string s);
    }
}
