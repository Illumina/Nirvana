using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsProteinNomenclature
    {
        public static string GetHgvsProteinAnnotation(
			ITranscript transcript, 
			string refAminoAcids,
			string altAminoAcids,
			string transcriptAltAllele,
			IMappedPositions mappedPositions, 
			VariantEffect variantEffect, 
			ISimpleVariant variant, 
			ISequence refSequence, 
			string hgvscNotation,
			bool isMitochondrial)
        {
	        if (IsHgvspNull(transcriptAltAllele, mappedPositions, variant, hgvscNotation)) return null;

	        var start    = mappedPositions.ProteinInterval.Start.Value;
	        var cdsStart = mappedPositions.CdsInterval.Start.Value;
			var cdsEnd   = mappedPositions.CdsInterval.End.Value;


			var peptideSeq = transcript.Translation.PeptideSeq;

			// Amino acid seq should never go past the stop codon
	        refAminoAcids = !refAminoAcids.EndsWith(AminoAcids.StopCodon) && refAminoAcids.Contains(AminoAcids.StopCodon)
		        ? refAminoAcids.Split(AminoAcids.StopCodon[0])[0]+AminoAcids.StopCodon
		        : refAminoAcids;

			HgvsUtilities.ShiftAndRotateAlleles( ref start, ref refAminoAcids, ref altAminoAcids, peptideSeq);

	        var end             = start + refAminoAcids.Length - 1;
	        var refAbbreviation = AminoAcids.GetAbbreviations(refAminoAcids);
	        var altAbbreviation = AminoAcids.GetAbbreviations(altAminoAcids);

			var proteinId     = transcript.Translation.ProteinId.WithVersion;
			var proteinChange = GetProteinChange(start, refAminoAcids, altAminoAcids, peptideSeq, variantEffect);


			switch (proteinChange)
			{
				case ProteinChange.Substitution:
					return HgvspNotation.GetSubstitutionNotation(proteinId, start, refAbbreviation, altAbbreviation);

				case ProteinChange.Unknown:
					//todo:not defined in hgvs standards
					return HgvspNotation.GetUnknownNotation(proteinId, start, end, refAbbreviation, altAbbreviation);

				case ProteinChange.Deletion:
					return HgvspNotation.GetDeletionNotation(proteinId, start, end, refAbbreviation, variantEffect.IsStopGained());

				case ProteinChange.Duplication:
					start -= altAminoAcids.Length;
					return HgvspNotation.GetDuplicationNotation(proteinId, start, end, altAbbreviation);

				case ProteinChange.Frameshift:
					return GetHgvsFrameshiftNotation(refSequence, cdsStart,
						cdsEnd, transcriptAltAllele, transcript, isMitochondrial, proteinId, start, end);

				case ProteinChange.None:
					return HgvspNotation.GetSilentNotation(hgvscNotation, start, refAbbreviation, variantEffect.IsStopRetained());

				case ProteinChange.DelIns:
					return HgvspNotation.GetDelInsNotation(proteinId, start, end, refAbbreviation, altAbbreviation);

				case ProteinChange.Insertion:
					Swap.Int(ref start, ref end);
					return HgvspNotation.GetInsertionNotation(proteinId, start, end, altAbbreviation, peptideSeq);
				
				case ProteinChange.Extension:
					var altPeptideSequence = HgvsUtilities.GetAltPeptideSequence(refSequence, cdsStart,
						cdsEnd, transcriptAltAllele, transcript, isMitochondrial);
					altAbbreviation = start <= altPeptideSequence.Length ? AminoAcids.ConvertAminoAcidToAbbreviation(altPeptideSequence[start - 1]): "Ter";
					var countToStop = HgvsUtilities.GetNumAminoAcidsUntilStopCodon(altPeptideSequence, peptideSeq, start - 1, false);

					return HgvspNotation.GetExtensionNotation(proteinId, start, refAbbreviation, altAbbreviation,countToStop);

				case ProteinChange.StartLost:
					return HgvspNotation.GetStartLostNotation(proteinId, start, end, refAbbreviation);
			}

			return null;
		}

        private static string GetHgvsFrameshiftNotation(ISequence refSequence, int cdsBegin, int cdsEnd,
            string transcriptAltAllele, ITranscript transcript, bool isMitochondrial, string proteinId, int start,
            int end)
        {
		    var peptideSeq = transcript.Translation.PeptideSeq;
		    var altPeptideSeq = HgvsUtilities.GetAltPeptideSequence(refSequence, cdsBegin, cdsEnd, transcriptAltAllele, transcript, isMitochondrial);

		    if (start > end) Swap.Int(ref start, ref end);

		    var frameshiftedParameters = HgvsUtilities.GetChangesAfterFrameshift(start, peptideSeq, altPeptideSeq);

			start            = frameshiftedParameters.Item1;
		    var refAminoAcid = frameshiftedParameters.Item2;
		    var altAminoAcid = frameshiftedParameters.Item3;

		    var refAbbreviation = AminoAcids.ConvertAminoAcidToAbbreviation(refAminoAcid);

			if (altAminoAcid == AminoAcids.StopCodonChar)
			    return HgvspNotation.GetSubstitutionNotation(proteinId, start, refAbbreviation, "Ter");
			
		    var altAbbreviation = AminoAcids.ConvertAminoAcidToAbbreviation(altAminoAcid);
		    var countToStop     = HgvsUtilities.GetNumAminoAcidsUntilStopCodon(altPeptideSeq, peptideSeq, start - 1, true);

		    return HgvspNotation.GetFrameshiftNotation(proteinId, start, refAbbreviation, altAbbreviation, countToStop);
	    }

	    private static bool IsHgvspNull(string transcriptAltAllele, IMappedPositions mappedPositions, ISimpleVariant variant, string hgvscNotation)
	    {
		    return string.IsNullOrEmpty(hgvscNotation)
		           || variant.Type == VariantType.reference 
		           || SequenceUtilities.HasNonCanonicalBase(transcriptAltAllele)
		           || mappedPositions.CdsInterval.Start == null 
		           || mappedPositions.CdsInterval.End == null;
	    }

        internal static ProteinChange GetProteinChange(int start, string refAminoAcids, string altAminoAcids,
            string peptideSeq, IVariantEffect variantEffect)
        {
		    if (refAminoAcids == altAminoAcids
				|| variantEffect.IsStopRetained()) return ProteinChange.None;

            //insertion before the transcript
            if(refAminoAcids.Length==0 && start==1) return ProteinChange.None;

		    if (variantEffect.IsStartLost()) return ProteinChange.StartLost;

			// todo: add start gained
		    // according to var nom, only if the Stop codon is effected, we call it an extension
			if (variantEffect.IsStopLost() && refAminoAcids.StartsWith(AminoAcids.StopCodon)) return ProteinChange.Extension;

			if (variantEffect.IsFrameshiftVariant()) return ProteinChange.Frameshift;

			
			if (altAminoAcids.Length > refAminoAcids.Length && HgvsUtilities.IsAminoAcidDuplicate(start, altAminoAcids, peptideSeq))
				return ProteinChange.Duplication;
		    
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
