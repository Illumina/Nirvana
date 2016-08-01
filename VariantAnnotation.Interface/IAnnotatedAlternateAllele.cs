using System;
using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IAnnotatedAlternateAllele
    {
        // permanent records
        string VariantId { get; }
        string VariantType { get; }

        string ReferenceName { get; }
        int? ReferenceBegin { get; }
        int? ReferenceEnd { get; }
        string RefAllele { get; }
        string AltAllele { get; }

        bool IsReferenceMinor { get; }
        bool IsIntergenic { get; }
        bool IsReference { get; }
        bool IsReferenceNoCall { get; }


        Tuple<int, int> CiPos { get; }
        Tuple<int, int> CiEnd { get; }

		// extensible records
		string PhylopScore { get; } // 11.612

		string AncestralAllele { get; }
		string EvsCoverage { get; }
		string EvsSamples { get; }
		string GlobalMinorAllele { get; }
		string GlobalMinorAlleleFrequency { get; }


		string AlleleFrequencyAll { get; }                      // ALL 0.1239
		string AlleleFrequencyAdMixedAmerican { get; }          // AMR 0.1239
		string AlleleFrequencyAfrican { get; }                  // AFR 0.1239
		string AlleleFrequencyEastAsian { get; }                // EAS 0.1239
		string AlleleFrequencyEuropean { get; }                 // EUR 0.1239
		string AlleleFrequencySouthAsian { get; }               // SAS 0.1239

	
		string OneKgAlleleNumberAfrican { get; }
		string OneKgAlleleNumberAmerican { get; }
		string OneKgAlleleNumberAll { get; }
		string OneKgAlleleNumberEastAsian { get; }
		string OneKgAlleleNumberEuropean { get; }
		string OneKgAlleleNumberSouthAsian { get; }

		string OneKgAlleleCountAfrican { get; }
		string OneKgAlleleCountAmerican { get; }
		string OneKgAlleleCountAll { get; }
		string OneKgAlleleCountEastAsian { get; }
		string OneKgAlleleCountEuropean { get; }
		string OneKgAlleleCountSouthAsian { get; }


		string EvsAlleleFrequencyAfricanAmerican { get; }       // African American 0.1239
		string EvsAlleleFrequencyEuropeanAmerican { get; }      // European American 0.1239
		string EvsAlleleFrequencyAll { get; }

		string ExacCoverage { get; }
		string ExacAlleleFrequencyAfrican { get; }
		string ExacAlleleFrequencyAmerican { get; }
		string ExacAlleleFrequencyAll { get; }
		string ExacAlleleFrequencyEastAsian { get; }
		string ExacAlleleFrequencyFinish { get; }
		string ExacAlleleFrequencyNonFinish { get; }
		string ExacAlleleFrequencyOther { get; }
		string ExacAlleleFrequencySouthAsian { get; }

		string ExacAlleleNumberAfrican { get; }
		string ExacAlleleNumberAmerican { get; }
		string ExacAlleleNumberAll { get; }
		string ExacAlleleNumberEastAsian { get; }
		string ExacAlleleNumberFinish { get; }
		string ExacAlleleNumberNonFinish { get; }
		string ExacAlleleNumberOther { get; }
		string ExacAlleleNumberSouthAsian { get; }

		string ExacAlleleCountAfrican { get; }
		string ExacAlleleCountAmerican { get; }
		string ExacAlleleCountAll { get; }
		string ExacAlleleCountEastAsian { get; }
		string ExacAlleleCountFinish { get; }
		string ExacAlleleCountNonFinish { get; }
		string ExacAlleleCountOther { get; }
		string ExacAlleleCountSouthAsian { get; }

		string[] DbSnpIds { get; }

		// -----------
		// transcripts
		// -----------

		IEnumerable<ITranscript> RefSeqTranscripts { get; }
        IEnumerable<ITranscript> EnsemblTranscripts { get; }
        IEnumerable<IRegulatoryRegion> RegulatoryRegions { get; }

        // -----------------
        // overlapping genes
        // -----------------

        IEnumerable<string> OverlappingGenes { get; }

        // -------------------------
        // Supplementary annotations
        // -------------------------

        IEnumerable<IClinVar> ClinVarEntries { get; }
        IEnumerable<ICosmic> CosmicEntries { get; }
        IEnumerable<ICustomAnnotation> CustomItems { get; }

        // ----------------------------
        // customIntervals
        // ----------------------------
        IEnumerable<ICustomInterval> CustomIntervals { get; }
    }
}
