using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class MappedPosition : IMappedPosition
    {
        public int CdnaStart           { get; }
        public int CdnaEnd             { get; }
        public int CdsStart            { get; }
        public int CdsEnd              { get; }
        public int ExtendedCdsEnd      { get; }
        public int ProteinStart        { get; set; }
        public int ProteinEnd          { get; set; }
        public int ExtendedProteinEnd  { get; }
        public int ExonStart           { get; }
        public int ExonEnd             { get; }
        public int IntronStart         { get; }
        public int IntronEnd           { get; }
        public int RegionStartIndex    { get; }
        public int RegionEndIndex      { get; }
        public int CoveredProteinStart { get; set; }
        public int CoveredProteinEnd   { get; set; }
        public int CoveredCdsStart     { get; set; }
        public int CoveredCdsEnd       { get; set; }
        public int CoveredCdnaStart    { get; set; }
        public int CoveredCdnaEnd      { get; set; }

        public MappedPosition(int cdnaStart, int cdnaEnd, int cdsStart, int cdsEnd, int extendedCdsEnd, int proteinStart, int proteinEnd,int extendedProteinEnd, int exonStart, int exonEnd, int intronStart, int intronEnd, int regionStartIndex, int regionEndIndex)
        {
            CdnaStart          = cdnaStart;
            CdnaEnd            = cdnaEnd;
            CdsStart           = cdsStart;
            CdsEnd             = cdsEnd;
            ExtendedCdsEnd     = extendedCdsEnd;
            ProteinStart       = proteinStart;
            ProteinEnd         = proteinEnd;
            ExtendedProteinEnd = extendedProteinEnd;
            ExonStart          = exonStart;
            ExonEnd            = exonEnd;
            IntronStart        = intronStart;
            IntronEnd          = intronEnd;
            RegionStartIndex   = regionStartIndex;
            RegionEndIndex     = regionEndIndex;
        }
    }
}