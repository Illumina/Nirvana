namespace VariantAnnotation.Interface.IO
{
	public interface IExtendedBinaryWriter
	{
		void Write(bool b);
		void Write(byte b);
		void Write(ushort us);
		void Write(string s);
	    void Write(int value);
        void Write(uint value);
		void WriteOpt(int value);
		void WriteOpt(long value);
		void WriteOptAscii(string s);
	}
}