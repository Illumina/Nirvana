using System;
using IO;

namespace VariantAnnotation.GenericScore
{
    public sealed class ReaderSettings
    {
        public readonly bool             IsPositional;
        public readonly EncoderType      EncoderType;
        public readonly IScoreEncoder    ScoreEncoder;
        public readonly ScoreJsonEncoder ScoreJsonEncoder;
        public readonly string[]         Nucleotides;
        public readonly int              BlockLength;

        public ushort BytesRequired => ScoreEncoder.BytesRequired;

        public ReaderSettings(
            bool isPositional,
            EncoderType encoderType,
            IScoreEncoder scoreEncoder,
            ScoreJsonEncoder scoreJsonEncoder,
            string[] nucleotides,
            int blockLength
        )
        {
            IsPositional     = isPositional;
            EncoderType      = encoderType;
            ScoreEncoder     = scoreEncoder;
            ScoreJsonEncoder = scoreJsonEncoder;
            BlockLength      = blockLength;
            Nucleotides      = nucleotides;
        }


        public static ReaderSettings Read(ExtendedBinaryReader reader)
        {
            bool isPositional = reader.ReadBoolean();
            var  encoderType  = (EncoderType) reader.ReadByte();
            IScoreEncoder scoreEncoder = encoderType switch
            {
                EncoderType.ZeroToOne => ZeroToOneScoreEncoder.Read(reader),
                EncoderType.Generic   => GenericScoreEncoder.Read(reader),
                _                     => throw new Exception("Unknown score encoder")
            };

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
                isPositional,
                encoderType,
                scoreEncoder,
                scoreJsonEncoder,
                nucleotides,
                blockLength
            );
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(IsPositional);
            writer.Write((byte) EncoderType);
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