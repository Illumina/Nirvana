namespace VariantAnnotation.Interface.IO
{
	public interface IExtendedBinaryWriter
	{
		void Write(bool b);
		void Write(byte b);
		void Write(string s);
	    void Write(ushort value);
        void Write(uint value);
	    void WriteOpt(ushort value);
		void WriteOpt(int value);
		void WriteOpt(long value);
		void WriteOptAscii(string s);
	}
}