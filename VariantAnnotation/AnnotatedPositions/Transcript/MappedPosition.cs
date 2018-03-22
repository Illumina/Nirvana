using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class MappedPosition : IMappedPosition
    {
        public int CdnaStart { get; }
        public int CdnaEnd { get; }
        public int CdsStart { get; }
        public int CdsEnd { get; }
        public int ProteinStart { get; }
        public int ProteinEnd { get; }
        public int ExonStart { get; }
        public int ExonEnd { get; }
        public int IntronStart { get; }
        public int IntronEnd { get; }
        public int RegionStartIndex { get; }
        public int RegionEndIndex { get; }

        public MappedPosition(int cdnaStart, int cdnaEnd, int cdsStart, int cdsEnd, int proteinStart, int proteinEnd,
            int exonStart, int exonEnd, int intronStart, int intronEnd, int regionStartIndex, int regionEndIndex)
        {
            CdnaStart        = cdnaStart;
            CdnaEnd          = cdnaEnd;
            CdsStart         = cdsStart;
            CdsEnd           = cdsEnd;
            ProteinStart     = proteinStart;
            ProteinEnd       = proteinEnd;
            ExonStart        = exonStart;
            ExonEnd          = exonEnd;
            IntronStart      = intronStart;
            IntronEnd        = intronEnd;
            RegionStartIndex = regionStartIndex;
            RegionEndIndex   = regionEndIndex;
        }
    }
}