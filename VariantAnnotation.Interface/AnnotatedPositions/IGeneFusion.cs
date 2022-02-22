using JSON;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IGeneFusion : IJsonSerializer
    {
        int? Exon { get; }
        int? Intron { get; }
        string HgvsCoding { get; }
    }
}