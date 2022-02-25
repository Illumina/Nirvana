using System;
using IO.v2;
using SAUtils.GenericScore.GenericScoreParser;
using VariantAnnotation.GenericScore;

namespace SAUtils.GenericScore
{
    public sealed class WriterSettings
    {
        public readonly Header Header      = new(FileType.GsaWriter, 1);
        public readonly Header IndexHeader = new(FileType.GsaIndex, 1);
        public readonly int    FilePairId  = new Random().Next(1_000_000, int.MaxValue);

        public readonly ScoreEncoder     ScoreEncoder;
        public readonly SaItemValidator  SaItemValidator;
        public readonly string[]         Nucleotides;
        public readonly int              BlockLength;
        public readonly ScoreJsonEncoder ScoreJsonEncoder;

        public WriterSettings(
            int blockLength,
            string[] nucleotides,
            ScoreEncoder scoreEncoder,
            ScoreJsonEncoder scoreJsonEncoder,
            SaItemValidator saItemValidator
        )
        {
            BlockLength      = blockLength;
            Nucleotides      = nucleotides;
            ScoreEncoder     = scoreEncoder;
            ScoreJsonEncoder = scoreJsonEncoder;
            SaItemValidator  = saItemValidator;
        }
    }
}