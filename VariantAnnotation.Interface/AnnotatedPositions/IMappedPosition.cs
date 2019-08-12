namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IMappedPosition
    {
        int CdnaStart { get; }
        int CdnaEnd { get; }
        int CdsStart { get; }
        int CdsEnd { get; }
        int ProteinStart { get; set; }
        int ProteinEnd { get; set; }
        int ExonStart { get; }
        int ExonEnd { get; }
        int IntronStart { get; }
        int IntronEnd { get; }
        int RegionStartIndex { get; }
        int RegionEndIndex { get; }
    }
}