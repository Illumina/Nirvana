using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Interface.Providers
{
	public interface IAnnotationProvider : IProvider
    {
		void Annotate(IAnnotatedPosition annotatedPosition);
	}
}