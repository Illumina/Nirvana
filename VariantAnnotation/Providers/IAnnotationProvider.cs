using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Providers
{
	public interface IAnnotationProvider : IProvider
    {
		void Annotate(AnnotatedPosition annotatedPosition);
    }
}