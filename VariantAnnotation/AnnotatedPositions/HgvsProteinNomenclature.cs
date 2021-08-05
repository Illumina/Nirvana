using Genome;
using OptimizedCore;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsProteinNomenclature
    {
	    public static string GetHgvsProteinAnnotation(ITranscript transcript, string refAminoAcids,
		    string altAminoAcids, string transcriptAltAllele, IMappedPosition position, VariantEffect variantEffect,
		    ISimpleVariant variant, ISequence refSequence, string hgvscNotation, AminoAcid aminoAcids)
	    {
		    if (IsHgvspNull(transcriptAltAllele, position.CdsStart, position.CdsEnd, variant, hgvscNotation))
			    return null;

		    string peptideSeq = transcript.Translation.PeptideSeq;

		    // Amino acid seq should never go past the stop codon
		    refAminoAcids = !refAminoAcids.EndsWith(AminoAcidCommon.StopCodon) &&
		                    refAminoAcids.Contains(AminoAcidCommon.StopCodon)
			    ? refAminoAcids.OptimizedSplit(AminoAcidCommon.StopCodon)[0] + AminoAcidCommon.StopCodon
			    : refAminoAcids;

		    int proteinStart = position.ProteinStart;
		    HgvsUtilities.ShiftAndRotateAlleles(ref proteinStart, ref refAminoAcids, ref altAminoAcids, peptideSeq);

		    int    end             = proteinStart + refAminoAcids.Length - 1;
		    string refAbbreviation = AminoAcidAbbreviation.ConvertToThreeLetterAbbreviations(refAminoAcids);
		    string altAbbreviation = AminoAcidAbbreviation.ConvertToThreeLetterAbbreviations(altAminoAcids);

		    string proteinId = transcript.Translation.ProteinId.WithVersion;
		    var proteinChange = GetProteinChange(proteinStart, refAminoAcids, altAminoAcids, peptideSeq, variantEffect);

		    // ReSharper disable once SwitchStatementMissingSomeCases
		    switch (proteinChange)
		    {
			    case ProteinChange.Substitution:
				    return HgvspNotation.GetSubstitutionNotation(proteinId, proteinStart, refAbbreviation,
					    altAbbreviation);

			    case ProteinChange.Unknown:
					return HgvspNotation.GetUnknownNotation(proteinId, proteinStart, end, refAbbreviation, altAbbreviation);

				case ProteinChange.Deletion:
					return HgvspNotation.GetDeletionNotation(proteinId, proteinStart, end, refAbbreviation, variantEffect.IsStopGained());

				case ProteinChange.Duplication:
				    proteinStart -= altAminoAcids.Length;
					return HgvspNotation.GetDuplicationNotation(proteinId, proteinStart, end, altAbbreviation);

				case ProteinChange.Frameshift:
				    return GetHgvsFrameshiftNotation(refSequence, position.CdsStart, position.CdsEnd, transcriptAltAllele,
				        transcript, aminoAcids, position.ProteinStart, proteinId, proteinStart, end);

				case ProteinChange.None:
					return HgvspNotation.GetSilentNotation(hgvscNotation, proteinStart, refAbbreviation, variantEffect.IsStopRetained());

				case ProteinChange.DelIns:
					return HgvspNotation.GetDelInsNotation(proteinId, proteinStart, end, refAbbreviation, altAbbreviation);

				case ProteinChange.Insertion:
					Swap.Int(ref proteinStart, ref end);
					return HgvspNotation.GetInsertionNotation(proteinId, proteinStart, end, altAbbreviation, peptideSeq);
				
				case ProteinChange.Extension:
					string altPeptideSequence = HgvsUtilities.GetAltPeptideSequence(refSequence, position.CdsStart,
						position.CdsEnd, transcriptAltAllele, transcript, aminoAcids, position.ProteinStart);
					
					altAbbreviation = proteinStart <= altPeptideSequence.Length
						? AminoAcidAbbreviation.GetThreeLetterAbbreviation(altPeptideSequence[proteinStart - 1])
						: "Ter";
					
					int countToStop = HgvsUtilities.GetNumAminoAcidsUntilStopCodon(altPeptideSequence, peptideSeq,
						proteinStart - 1, false);

					return HgvspNotation.GetExtensionNotation(proteinId, proteinStart, refAbbreviation, altAbbreviation,countToStop);

				case ProteinChange.StartLost:
					return HgvspNotation.GetStartLostNotation(proteinId, refAbbreviation);
			}

			return null;
		}

	    private static string GetHgvsFrameshiftNotation(ISequence refSequence, int cdsBegin, int cdsEnd,
		    string transcriptAltAllele, ITranscript transcript, AminoAcid aminoAcids, int aaBegin, string proteinId,
		    int start, int end)
        {
	        string peptideSeq = transcript.Translation.PeptideSeq;
	        string altPeptideSeq = HgvsUtilities.GetAltPeptideSequence(refSequence, cdsBegin, cdsEnd,
		        transcriptAltAllele, transcript, aminoAcids, aaBegin);

	        if (start > end) Swap.Int(ref start, ref end);

	        char refAminoAcid, altAminoAcid;
	        (start, refAminoAcid, altAminoAcid) =
		        HgvsUtilities.GetChangesAfterFrameshift(start, peptideSeq, altPeptideSeq);

	        string refAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(refAminoAcid);

	        if (altAminoAcid == AminoAcidCommon.StopCodon)
		        return HgvspNotation.GetSubstitutionNotation(proteinId, start, refAbbreviation, "Ter");

	        string altAbbreviation = AminoAcidAbbreviation.GetThreeLetterAbbreviation(altAminoAcid);
	        int countToStop = HgvsUtilities.GetNumAminoAcidsUntilStopCodon(altPeptideSeq, peptideSeq, start - 1, true);

	        return HgvspNotation.GetFrameshiftNotation(proteinId, start, refAbbreviation, altAbbreviation, countToStop);
        }

        private static bool IsHgvspNull(string transcriptAltAllele, int cdsStart, int cdsEnd, ISimpleVariant variant,
            string hgvscNotation)
        {
            return string.IsNullOrEmpty(hgvscNotation)                        ||
                   variant.Type == VariantType.reference                      ||
                   SequenceUtilities.HasNonCanonicalBase(transcriptAltAllele) ||
                   cdsStart == -1                                             || 
                   cdsEnd == -1;
        }

        internal static ProteinChange GetProteinChange(int start, string refAminoAcids, string altAminoAcids,
            string peptideSeq, IVariantEffect variantEffect)
        {
            bool insertionBeforeTranscript = refAminoAcids.Length == 0 && start == 1;
            if (refAminoAcids == altAminoAcids || variantEffect.IsStopRetained() || insertionBeforeTranscript) return ProteinChange.None;

            if (variantEffect.IsStartLost()) return ProteinChange.StartLost;

            // according to var nom, only if the Stop codon is effected, we call it an extension
            if (variantEffect.IsStopLost() && refAminoAcids.OptimizedStartsWith(AminoAcidCommon.StopCodon)) return ProteinChange.Extension;

            if (variantEffect.IsFrameshiftVariant()) return ProteinChange.Frameshift;

            if (altAminoAcids.Length > refAminoAcids.Length &&
                HgvsUtilities.IsAminoAcidDuplicate(start, altAminoAcids, peptideSeq)) return ProteinChange.Duplication;

            if (refAminoAcids.Length == 0 && altAminoAcids.Length != 0) return ProteinChange.Insertion;

            if (refAminoAcids.Length != 0 && altAminoAcids.Length == 0) return ProteinChange.Deletion;

            if (refAminoAcids.Length == 1 && altAminoAcids.Length == 1) return ProteinChange.Substitution;

            // the only remaining possibility is deletions/insertions
            return ProteinChange.DelIns;
        }
    }

    public enum ProteinChange
    {
        Unknown,
        Deletion,
        Duplication,
        Frameshift,
        DelIns,
        Insertion,
        None,
		Extension,
		StartLost,
        Substitution
    }
}
