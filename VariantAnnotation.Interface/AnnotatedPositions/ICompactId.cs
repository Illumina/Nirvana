using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public interface ICompactId:ISerializable
	{
		IdType Id{ get; }
		int Info{ get; }

	}

	public enum IdType : byte
	{
	    // ReSharper disable InconsistentNaming
        Unknown,
		Ccds,
		EnsemblGene,
		EnsemblTranscript,
		EnsemblProtein,
		EnsemblEstGene,
		EnsemblRegulatory,	    
		RefSeqNonCodingRNA,	    
		RefSeqMessengerRNA,
		RefSeqProtein,
		RefSeqPredictedNonCodingRNA,
		RefSeqPredictedMessengerRNA,
		RefSeqPredictedProtein,
		OnlyNumbers
	    // ReSharper restore InconsistentNaming
    }
}