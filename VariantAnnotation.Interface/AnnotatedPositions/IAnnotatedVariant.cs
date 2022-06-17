using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface.SA;
using Variants;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public interface IAnnotatedVariant
	{
		IVariant                          Variant                   { get; }
        string                            HgvsgNotation             { get; set; }
        IList<IAnnotatedRegulatoryRegion> RegulatoryRegions         { get;  }
	    IList<IAnnotatedTranscript>       Transcripts               { get; }
        IList<ISupplementaryAnnotation>   SaList                    { get; }
        ISupplementaryAnnotation          RepeatExpansionPhenotypes { get; set; }
		double?                           PhylopScore               { get; set; }
		double?                           GerpScore               { get; set; }
		bool                              InLowComplexityRegion     { get; set; }
        StringBuilder                     GetJsonStringBuilder(string originalChromName);
    }	
}