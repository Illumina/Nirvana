namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IGeneFusionAnnotation
    {
        int? Exon { get; }
        int? Intron { get; }
        IGeneFusion[] GeneFusions { get; }
    }
}