namespace VariantAnnotation.Interface
{
	public interface IAnnotationSourceFactory
	{
		/// <summary>
		/// Creates an annotation source and returns it. The annotation source performs annotation on variants
		/// </summary>
		/// <param name="annotatorInfo">Parameters to initialize the annotator like Sample names, etc.</param>
		/// <param name="annotatorPaths">Relevant paths like compressed reference, supplementary annotation, etc.</param>
		/// <returns>An annotation source factory</returns>
		IAnnotationSource CreateAnnotationSource(IAnnotatorInfo annotatorInfo, IAnnotatorPaths annotatorPaths);
	}
}
