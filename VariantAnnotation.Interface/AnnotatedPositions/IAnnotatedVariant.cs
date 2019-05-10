using System.Collections.Generic;
using VariantAnnotation.Interface.SA;
using Variants;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public interface IAnnotatedVariant
	{
		IVariant Variant { get; }
        string HgvsgNotation { get; set; }
        IList<IAnnotatedRegulatoryRegion> RegulatoryRegions { get;  }
	    IList<IAnnotatedTranscript> Transcripts { get; }
        IList<ISupplementaryAnnotation> SaList { get; }
		double? PhylopScore { get; set; }

	    IList<IPluginData> PluginDataSet { get; }
        string GetJsonString(string originalChromName);
    }	
}