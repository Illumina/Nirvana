namespace VariantAnnotation.Algorithms.Consequences
{
    // tier - variations are assigned consequences in tier order. if a tier 1 consequence is assigned,
    //        no tier 2 consequences will be checked/assigned
    //
    // rank - the relative rank of this consequence type when compared to other consequence results
    public enum ConsequenceType : byte
    {
        Unknown,
        TranscriptAblation,                     // tier 1, rank 1
        TranscriptAmplification,                // tier 1, rank 8
        MatureMirnaVariant,                     // tier 2, rank 17
        //TfbsAblation,                           // tier 2, rank 26 (not used)
        //TfbsAmplification,                      // tier 2, rank 28 (not used)
        //TfBindingSiteVariant,                   // tier 2, rank 30 (not used)
        RegulatoryRegionAblation,               // tier 2, rank 31
        RegulatoryRegionAmplification,          // tier 2, rank 33
        RegulatoryRegionVariant,                // tier 2, rank 36
        SpliceDonorVariant,                     // tier 3, rank 3
        SpliceAcceptorVariant,                  // tier 3, rank 3
        StopGained,                             // tier 3, rank 4
        FrameshiftVariant,                      // tier 3, rank 5
        StopLost,                               // tier 3, rank 6
        StartLost,                              // tier 3, rank 7
        InframeInsertion,                       // tier 3, rank 10
        InframeDeletion,                        // tier 3, rank 11
        MissenseVariant,                        // tier 3, rank 12
        ProteinAlteringVariant,                 // tier 3, rank 12
        SpliceRegionVariant,                    // tier 3, rank 13
        IncompleteTerminalCodonVariant,         // tier 3, rank 14
        StopRetainedVariant,                    // tier 3, rank 15
        SynonymousVariant,                      // tier 3, rank 15
        CodingSequenceVariant,                  // tier 3, rank 16
        FivePrimeUtrVariant,                    // tier 3, rank 18
        ThreePrimeUtrVariant,                   // tier 3, rank 19
        NonCodingTranscriptExonVariant,         // tier 3, rank 20
        IntronVariant,                          // tier 3, rank 21
        NonsenseMediatedDecayTranscriptVariant, // tier 3, rank 22
        NonCodingTranscriptVariant,             // tier 3, rank 23
        UpstreamGeneVariant,                    // tier 3, rank 24
        DownstreamGeneVariant,                  // tier 3, rank 25
        FeatureElongation,                      // tier 3, rank 36
        FeatureTruncation,                      // tier 3, rank 37
        TranscriptTruncation,
        CopyNumberIncrease,                         // tier CNV
        CopyNumberDecrease,                         // tier CNV
        CopyNumberChange,                    // tier CNV
        //IntergenicVariant,                      // tier 4, rank 38 (not used)
		GeneFusion,								//tier breakend
        ShortTandemRepeatChange,
        ShortTandemRepeatExpansion,
        ShortTandemRepeatContraction,
        TranscriptVariant                      //default for variant overlap with transcript	
	}
}
