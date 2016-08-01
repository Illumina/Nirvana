
using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
	public interface IAnnotationSource
	{
		/// <summary>
		/// Performs annotation on a variant and returns an annotated variant
		/// </summary>
		/// <param name="variant">a variant object</param>
		/// <returns>an annotated variant</returns>
		IAnnotatedVariant Annotate(IVariant variant);

		IEnumerable<IDataSourceVersion> GetDataSourceVersions();

        /// <summary>
        /// adds custom intervals to the annotation source
        /// </summary>
	    void AddCustomIntervals(IEnumerable<ICustomInterval> customIntervals);

        /// <summary>
        /// adds supplementary intervals to the annotation source
        /// </summary>
        void AddSupplementaryIntervals(IEnumerable<ISupplementaryInterval> supplementaryIntervals);

        /// <summary>
        /// disables the annotation loader (useful when unit testing)
        /// </summary>
	    void DisableAnnotationLoader();

        /// <summary>
        /// enables the annotation of the mitochondrial genome
        /// </summary>
	    void EnableReferenceNoCalls(bool limitReferenceNoCallsToTranscripts);

        /// <summary>
        /// enables the annotation of the mitochondrial genome
        /// </summary>
        void EnableMitochondrialAnnotation();

        string GetDataVersion();

        /// <summary>
        /// finalizes the annotator metrics before disposal
        /// </summary>
	    void FinalizeMetrics();
	}
}
