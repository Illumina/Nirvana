
using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
	public interface IAnnotationSourceFactory
	{
		/// <summary>
		/// Creates an annotation source and returns it. The annotation source performs annotation on variants
		/// </summary>
		/// <param name="annotatorInfo">Parameters to initialize the annotator like Sample names, etc.</param>
		/// <param name="annotatorPaths">Relevanat paths like compressed reference, supplementary annotation, etc.</param>
		/// <returns>An annotation source factory</returns>
		IAnnotationSource CreateAnnotationSource(IAnnotatorInfo annotatorInfo, IAnnotatorPaths annotatorPaths);

	}
	/// <summary>
	/// Encapsulates all the various paths required to initialize an annotation source
	/// </summary>
	public interface IAnnotatorPaths
	{
		string CachePrefix { get; }
		string CompressedReference { get; }
		string SupplementaryAnnotation { get; }
		IEnumerable<string> CustomAnnotation { get; }
		IEnumerable<string> CustomIntervals { get; }
	}

	/// <summary>
	/// encapsulates info fields needed for initializing the annotation source
	/// </summary>
	public interface IAnnotatorInfo
	{
		// The name of the samples to be annotated
		IEnumerable<string> SampleNames { get; }
		IEnumerable<string> BooleanArguments { get; } 
	}

    //public interface IAnnotationSourceReaders
    //{
    //    IReader saReader { get; }
    //    IReader conservationScoreReader { get; }
    //    IReader compressedSequenceReader { get; }
    //}
}
