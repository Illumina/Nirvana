namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IMappedPosition
    {
        int ProteinStart { get; set; }
        int ProteinEnd { get; set; }
        int CdsStart { get; }
        int CdsEnd { get; }
        int CdnaStart { get; }
        int CdnaEnd { get; }
        int ExonStart { get; }
        int ExonEnd { get; }
        int IntronStart { get; }
        int IntronEnd { get; }
        int RegionStartIndex { get; }
        int RegionEndIndex { get; }
        int CoveredProteinStart { get; set; }
        int CoveredProteinEnd { get; set; }
        int CoveredCdsStart { get; set; }
        int CoveredCdsEnd { get; set; }
        int CoveredCdnaStart { get; set; }
        int CoveredCdnaEnd { get; set; }
    }
}