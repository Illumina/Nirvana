using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public interface IAnnotatedPosition
	{
		IPosition Position { get; }
		string CytogeneticBand { get; set; }
		IAnnotatedVariant[] AnnotatedVariants { get; }
		IList<ISupplementaryAnnotation> SupplementaryIntervals { get; }
	    string GetJsonString();
    }
}