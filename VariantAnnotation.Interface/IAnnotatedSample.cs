using System;
using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
	public interface IAnnotatedSample
	{

		string Genotype { get; }         // 0/1 (GT)
		string VariantFrequency { get; } // 1.00 (AF)
		string TotalDepth { get; }       // 10 (DP)
		bool FailedFilter { get; }       // F (FT)
		string[] AlleleDepths { get; }   // 92,21 (AD)
		string GenotypeQuality { get; }  // 790 (GQX)
		string CopyNumber { get; }//CN in CANVAs
		bool IsLossOfHeterozygosity { get; }
		string DenovoQuality { get; }
		string RepeatNumber { get; }
        string RepeatNumberSpan { get; }
		bool IsEmpty { get; }
        List<String> RecomposedGenotype { get; }
	}
}
