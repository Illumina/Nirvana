namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public interface IVariantEffect
	{
		bool IsStopLost();
		bool IsStopRetained();
		bool IsStartLost();
		bool IsFrameshiftVariant();
	    bool IsMatureMirnaVariant();
	    bool IsSpliceDonorVariant();
	    bool IsSpliceAcceptorVariant();
	    bool IsStopGained();
	    bool IsInframeInsertion();
	    bool IsInframeDeletion();
	    bool IsMissenseVariant();
	    bool IsProteinAlteringVariant();
	    bool IsSpliceRegionVariant();
	    bool IsIncompleteTerminalCodonVariant();
	    bool IsStartRetained();
	    bool IsSynonymousVariant();
	    bool IsCodingSequenceVariant();
	    bool IsFivePrimeUtrVariant();
	    bool IsThreePrimeUtrVariant();
	    bool IsNonCodingTranscriptExonVariant();
	    bool IsWithinIntron();
	    bool IsNonsenseMediatedDecayTranscriptVariant();
	    bool IsNonCodingTranscriptVariant();
	}
}