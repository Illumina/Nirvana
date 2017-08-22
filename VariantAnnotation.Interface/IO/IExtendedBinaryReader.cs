namespace VariantAnnotation.Interface.IO
{
	public interface IExtendedBinaryReader
	{
		bool ReadBoolean();
		byte ReadByte();
		ushort ReadUInt16();
		string ReadString();
		uint ReadUInt32();
		long ReadOptInt64();
		string ReadAsciiString();
		int ReadOptInt32();
	}
}