﻿using System.Collections.Generic;

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
		IList<string> AlternateAlleles { get; }

		// optional
		string[] Filters { get; }
		string Quality { get; }
		string CytogeneticBand { get; }

		// now we place the samples and variant objects
		string StrandBias { get; }
		string RecalibratedQuality { get; }
		string JointSomaticNormalQuality { get; }
		string CopyNumber { get; }
		string Depth { get; }
		bool ColocalizedWithCnv { get; }

		string[] CiPos { get; }
		string[] CiEnd { get; }

		int? SvLength { get; }


		IEnumerable<IAnnotatedSample> AnnotatedSamples { get; }
		IList<IAnnotatedAlternateAllele> AnnotatedAlternateAlleles { get; }
		IEnumerable<IAnnotatedSupplementaryInterval> SupplementaryIntervals { get; }
	}

	public interface IAnnotatedSupplementaryInterval
	{
		#region members

		ISupplementaryInterval SupplementaryInterval { get; }
		double? ReciprocalOverlap { get; }

		#endregion
	}
	
}
