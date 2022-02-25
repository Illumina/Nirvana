using IO;

namespace VariantAnnotation.GenericScore
{
    public sealed class ReaderSettings
    {
        public readonly ScoreEncoder     ScoreEncoder;
        public readonly ScoreJsonEncoder ScoreJsonEncoder;
        public readonly string[]         Nucleotides;
        public readonly int              BlockLength;
        public          ushort           BytesRequired => ScoreEncoder.BytesRequired;

        public ReaderSettings(
            ScoreEncoder scoreEncoder,
            ScoreJsonEncoder scoreJsonEncoder,
            string[] nucleotides,
            int blockLength
        )
        {
            ScoreEncoder     = scoreEncoder;
            ScoreJsonEncoder = scoreJsonEncoder;
            BlockLength      = blockLength;
            Nucleotides      = nucleotides;
        }


        public static ReaderSettings Read(ExtendedBinaryReader reader)
        {
            ScoreEncoder     scoreEncoder     = ScoreEncoder.Read(reader);
            ScoreJsonEncoder scoreJsonEncoder = ScoreJsonEncoder.Read(reader);

            byte nucleotideCount = reader.ReadByte();
            var  nucleotides     = new string[nucleotideCount];

            for (var i = 0; i < nucleotideCount; i++)
            {
                string value = reader.ReadAsciiString();
                nucleotides[i] = value;
            }

            int blockLength = reader.ReadOptInt32();

            return new ReaderSettings(
                scoreEncoder,
                scoreJsonEncoder,
                nucleotides,
                blockLength
            );
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            ScoreEncoder.Write(writer);
            ScoreJsonEncoder.Write(writer);

            writer.Write((byte) Nucleotides.Length);
            foreach (string key in Nucleotides)
            {
                writer.WriteOptAscii(key);
            }

            writer.WriteOpt(BlockLength);
        }
    }
}