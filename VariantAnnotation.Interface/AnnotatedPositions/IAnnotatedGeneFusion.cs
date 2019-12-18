using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IAnnotatedGeneFusion : IJsonSerializer
    {
        int? Exon { get; }
        int? Intron { get; }
        IGeneFusion[] GeneFusions { get; }
    }
}