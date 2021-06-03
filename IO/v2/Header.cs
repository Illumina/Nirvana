using System.IO;

namespace IO.v2
{
    /// <summary>
    /// Common header for all our Nirvana file formats 
    /// </summary>
    public record Header(FileType FileType, ushort FileFormatVersion)
    {
        // see http://www.libpng.org/pub/png/spec/1.2/PNG-Rationale.html#R.PNG-file-signature
        
        // decimal            137  78  73  82  13  10   26  10
        // hexadecimal         89  4E  49  52  0d  0a   1a  0a
        // ASCII C notation  \211   N   I   R  \r  \n \032  \n
        private const ulong NirvanaSignature = 727905342105144969;

        public static Header Read(BinaryReader reader)
        {
            ulong  signature         = reader.ReadUInt64();
            var    fileType          = (FileType) reader.ReadUInt16();
            ushort fileFormatVersion = reader.ReadUInt16();

            if (signature != NirvanaSignature)
                throw new InvalidDataException("Invalid Nirvana file signature. Is this the correct file?");

            return new Header(fileType, fileFormatVersion);
        }
        
        public void Write(BinaryWriter writer)
        {
            writer.Write(NirvanaSignature);
            writer.Write((ushort) FileType);
            writer.Write(FileFormatVersion);
        }
    }
}