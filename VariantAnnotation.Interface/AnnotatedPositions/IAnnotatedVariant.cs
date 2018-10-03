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
	    IList<IAnnotatedTranscript> EnsemblTranscripts { get; }
	    IList<IAnnotatedTranscript> RefSeqTranscripts { get; }
	    IList<IAnnotatedSaDataSource> SupplementaryAnnotations { get; }
	    IList<ISupplementaryAnnotation> SaList { get; }
	    ISet<string> OverlappingGenes { get; }
        IList<IOverlappingTranscript>  OverlappingTranscripts { get; }
		double? PhylopScore { get; set; }

	    IList<IPluginData> PluginDataSet { get; }
        string GetJsonString(string originalChromName);
    }	
}