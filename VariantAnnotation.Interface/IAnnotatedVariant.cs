using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
	/// <summary>
	/// An interface for an annotated variant.
	/// An annotated variant is the result of an annotation process on a variant. 
	/// </summary>
	public interface IAnnotatedVariant
	{
		// mandatory positional fields
		string ReferenceName{ get; }
		int? ReferenceBegin{ get; }
		string ReferenceAllele{ get; }
		IEnumerable<string> AlternateAlleles{ get; }

		// optional

		string CytogeneticBand { get; }

		// now we place the samples and variant objects
		string StrandBias { get; }
		string RecalibratedQuality { get; }
		string JointSomaticNormalQuality { get; }
		string CopyNumber { get; }

		IEnumerable<IAnnotatedSample> AnnotatedSamples { get; }
		IEnumerable<IAnnotatedAlternateAllele> AnnotatedAlternateAlleles { get; }
		IEnumerable<IAnnotatedSupplementaryInterval> SupplementaryIntervals { get; }
	}
}
