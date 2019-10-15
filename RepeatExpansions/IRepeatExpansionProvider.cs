using VariantAnnotation.Interface.AnnotatedPositions;

namespace RepeatExpansions
{
    public interface IRepeatExpansionProvider
    {
        void Annotate(IAnnotatedPosition annotatedPosition);
    }
}
