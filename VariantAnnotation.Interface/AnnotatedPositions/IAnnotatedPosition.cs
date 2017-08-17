using System.Collections.Generic;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public interface IAnnotatedPosition
	{
		IPosition Position { get; }
		string CytogeneticBand { get; set; }
		IAnnotatedVariant[] AnnotatedVariants { get; }
		IList<IAnnotatedSupplementaryInterval> SupplementaryIntervals { get; }
	    string GetJsonString();
    }
}