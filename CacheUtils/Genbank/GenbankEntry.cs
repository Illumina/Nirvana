using Intervals;

namespace CacheUtils.Genbank
{
    public sealed class GenbankEntry
    {
        public readonly string TranscriptId;
        public readonly byte TranscriptVersion;
        public readonly string ProteinId;
        public readonly byte ProteinVersion;
        public readonly string GeneId;
        public readonly string Symbol;
        public readonly IInterval CodingRegion;
        public readonly IInterval[] Exons;

        public GenbankEntry(string transcriptId, byte transcriptVersion, string proteinId, byte proteinVersion,
            string geneId, string symbol, IInterval codingRegion, IInterval[] exons)
        {
            TranscriptId      = transcriptId;
            TranscriptVersion = transcriptVersion;
            ProteinId         = proteinId;
            ProteinVersion    = proteinVersion;
            GeneId            = geneId;
            Symbol            = symbol;
            CodingRegion      = codingRegion;
            Exons             = exons;
        }
    }
}
