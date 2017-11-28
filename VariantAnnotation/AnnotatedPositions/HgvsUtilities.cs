using System;
using System.Text;
using CommonUtilities;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsUtilities
    {
        public static void ShiftAndRotateAlleles(ref int start, ref string refAminoAcids, ref string altAminoAcids, string peptideSeq)
        {
            var trimmedAlleles = BiDirectionalTrimmer.Trim(start, refAminoAcids, altAminoAcids);

            start         = trimmedAlleles.Start;
            refAminoAcids = trimmedAlleles.RefAllele;
            altAminoAcids = trimmedAlleles.AltAllele;

            var rotatedAlleles = Rotate3Prime(refAminoAcids, altAminoAcids, start, peptideSeq);

            start         = rotatedAlleles.Start;
            refAminoAcids = rotatedAlleles.RefAminoAcids;
            altAminoAcids = rotatedAlleles.AltAminoAcids;
        }

        internal static (int Start, string RefAminoAcids, string AltAminoAcids) Rotate3Prime(string refAminoAcids, string altAminoAcids, int start, string peptides)
        {
            if (!(string.IsNullOrEmpty(refAminoAcids) || string.IsNullOrEmpty(altAminoAcids)))
                return (start, refAminoAcids, altAminoAcids);

            var isInsertion = !string.IsNullOrEmpty(altAminoAcids);

            // ReSharper disable once PossibleNullReferenceException
            var end = start + refAminoAcids.Length - 1;

            // for insertion, the reference bases will be empty string. The shift should happen on the alternate allele
            var rotatingPeptides = isInsertion ? altAminoAcids : refAminoAcids;
            var numBases = rotatingPeptides.Length;

            var downstreamPeptides = peptides.Length >= end ? peptides.Substring(end) : null;
            var combinedSequence   = rotatingPeptides + downstreamPeptides;

            int shiftStart, shiftEnd;
            var hasShifted = false;

            for (shiftStart = 0, shiftEnd = numBases; shiftEnd < combinedSequence.Length; shiftStart++, shiftEnd++)
            {
                if (combinedSequence[shiftStart] != combinedSequence[shiftEnd]) break;
                start++;
                hasShifted = true;
            }

            if (hasShifted) rotatingPeptides = combinedSequence.Substring(shiftStart, numBases);

            if (isInsertion) altAminoAcids = rotatingPeptides;
            else refAminoAcids = rotatingPeptides;

            return (start, refAminoAcids, altAminoAcids);
        }

        /// <summary>
        /// returns true if this insertion has the same amino acids preceding it [TranscriptVariationAllele.pm:1494 _check_for_peptide_duplication]
        /// </summary>
        public static bool IsAminoAcidDuplicate(int start, string altAminoAcids, string transcriptPeptides)
        {
            if (altAminoAcids == null || transcriptPeptides == null) return false;

            var testAminoAcidPos = start - altAminoAcids.Length - 1;
            if (testAminoAcidPos < 0) return false;

            var precedingAminoAcids = testAminoAcidPos + altAminoAcids.Length <= transcriptPeptides.Length
                ? transcriptPeptides.Substring(testAminoAcidPos, altAminoAcids.Length)
                : "";

            return testAminoAcidPos >= 0 && precedingAminoAcids == altAminoAcids;
        }

        /// <summary>
        /// returns the number of amino acids until the next stop codon is encountered [TranscriptVariationAllele.pm:1531 _stop_loss_extra_AA]
        /// </summary>
        public static int GetNumAminoAcidsUntilStopCodon(string altCds, string peptideSeq, int refVarPos, bool isFrameshift)
        {
            var numExtraAminoAcids = -1;
            var refLen = peptideSeq.Length;

            // find the number of residues that are translated until a termination codon is encountered
            var terPos = altCds.IndexOf('*');
            if (terPos != -1)
            {
                numExtraAminoAcids = terPos + 1 - (isFrameshift ? refVarPos : refLen + 1);
            }

            // A special case is if the first aa is a stop codon => don't display the number of residues until the stop codon
            return numExtraAminoAcids > 0 ? numExtraAminoAcids : -1;
        }

        public static (int Start, char RefAminoAcid, char AltAminoAcid) GetChangesAfterFrameshift(int start, string peptideSeq, string altPeptideSeq)
        {
	        start = Math.Min(start, peptideSeq.Length);

			// for deletions at the end of peptide sequence
	        if (start > altPeptideSeq.Length)
	        {
		        return (start, peptideSeq[start - 1], '?');
	        }
	        var refPeptideSeq = peptideSeq + "*";
            char refAminoAcid = refPeptideSeq[start - 1];
            char altAminoAcid = altPeptideSeq[start - 1];

            while (start <= altPeptideSeq.Length && start <= refPeptideSeq.Length)
            {
                refAminoAcid = refPeptideSeq[start - 1];
                altAminoAcid = altPeptideSeq[start - 1];

                // variation at stop codon, but maintains stop codon - set to synonymous
                if (refAminoAcid == '*' && altAminoAcid == '*' || refAminoAcid != altAminoAcid) break;
                start++;
            }

            return (start, refAminoAcid, altAminoAcid);
        }

        /// <summary>
        /// returns the translated coding sequence including the variant and the 3' UTR
        /// </summary>
        public static string GetAltPeptideSequence(ISequence refSequence, int cdsBegin, int cdsEnd,
            string trancriptAltAllele, ITranscript transcript, bool isMitochondrial)
        {
            string altCds = TranscriptUtilities.GetAlternateCds(refSequence, cdsBegin,
                cdsEnd, trancriptAltAllele, transcript.CdnaMaps,
                transcript.Gene.OnReverseStrand, transcript.StartExonPhase,
                transcript.Translation.CodingRegion.CdnaStart);

            var aminoAcids = new AminoAcids(isMitochondrial);
	        return aminoAcids.TranslateBases(altCds, true);
        }

	    /// <summary>
	    /// Return the coordinates of the variant reletive to the interval
	    /// </summary>
	    /// <param name="variant"></param>
	    /// <param name="interval"></param>
	    /// <param name="onReverseStrand"></param>
	    /// <returns>interval reflecting the relative coordinates</returns>
	    public static IInterval GetReletiveCoordinates(ISimpleVariant variant, IInterval interval, bool onReverseStrand)
	    {
		    // calculate the HGVS position: use HGVS coordinates not variation feature coordinates due to duplications
		    int refEnd, refStart;
		    if (onReverseStrand)
		    {
			    refStart = interval.End - variant.End + 1;
			    refEnd = interval.End - variant.Start + 1;
		    }
		    else
		    {
			    refStart = variant.Start - interval.Start + 1;
			    refEnd = variant.End - interval.Start + 1;
		    }

		    return new Interval(refStart, refEnd);
	    }

	    /// <summary>
	    /// gets the variant position (with intron offset) in the transcript [TranscriptVariationAllele.pm:1805 _get_cDNA_position]
	    /// </summary>
	    public static PositionOffset GetCdnaPositionOffset(ITranscript transcript, int position,bool isGenomicPosition = false)
	    {
            // start and stop coordinate relative to transcript. Take into account which
            // strand we're working on
	        if (!isGenomicPosition)
	            position = transcript.Gene.OnReverseStrand
	                ? transcript.End - position + 1
	                : transcript.Start + position - 1;

		    if (!transcript.Overlaps(position, position)) return null;

		    var po = new PositionOffset(position);

		    var exons = transcript.CdnaMaps;

		    for (int exonIndex = 0; exonIndex < exons.Length; exonIndex++)
		    {
			    var exon = exons[exonIndex];
			    if (position > exon.End) continue;

			    // EXONIC: if the start coordinate is within this exon
			    if (position >= exon.Start)
			    {
				    // get the cDNA start coordinate of the exon and add the number of nucleotides
				    // from the exon boundary to the variation. If the transcript is in the opposite
				    // direction, count from the end instead
				    po.Position = exon.CdnaStart + (transcript.Gene.OnReverseStrand
					                  ? exon.End - position
					                  : position - exon.Start);

				    break;
			    }

			    // INTRONIC: the start coordinate is between this exon and the previous one, determine which one is closest and get coordinates relative to that one

			    // sanity check: make sure we have at least passed one exon
			    if (exonIndex < 1)
			    {
				    //po.Position = null;
				    return null;
			    }

			    var prevExon = exons[exonIndex - 1];
			    GetIntronOffset(prevExon, exon, position, po, transcript.Gene.OnReverseStrand);
			    break;
		    }

		    // start by correcting for the stop codon
		    int startCodon = transcript.Translation?.CodingRegion.CdnaStart ?? -1;
		    int stopCodon = transcript.Translation?.CodingRegion.CdnaEnd ?? -1;

		    string cdnaCoord = po.Position.ToString();
		    po.HasStopCodonNotation = false;
		    bool hasNoPosition = false;

		    if (stopCodon != -1)
		    {
			    if (po.Position > stopCodon)
			    {
				    cdnaCoord = '*' + (po.Position - stopCodon).ToString();
				    po.HasStopCodonNotation = true;
			    }
			    else if (po.Offset != null && po.Position == stopCodon)
			    {
				    cdnaCoord = "*";
				    po.HasStopCodonNotation = true;
				    hasNoPosition = true;
			    }
		    }

		    if (!po.HasStopCodonNotation && startCodon != -1)
		    {
			    cdnaCoord = (po.Position + (po.Position >= startCodon ? 1 : 0) - startCodon).ToString();
		    }

		    // re-assemble the cDNA position  [ return exon num & offset & direction for intron eg. 142+363]
		    if (hasNoPosition) po.Value = "*" + po.Offset;
		    else po.Value = cdnaCoord + (po.Offset == null ? "" : ((int)po.Offset).ToString("+0;-0;+0"));

		    return po;
	    }

	    /// <summary>
	    /// get the shorted intron offset from the nearest exon
	    /// </summary>
	    public static void GetIntronOffset(ICdnaCoordinateMap prevExon, ICdnaCoordinateMap exon, int? position, PositionOffset po, bool onReverseStrand)
	    {
		    int? upDist = position - prevExon.End;
		    int? downDist = exon.Start - position;

		    if (upDist < downDist || upDist == downDist && !onReverseStrand)
		    {
			    // distance to upstream exon is the shortest (or equal and in the positive orientation)
			    if (onReverseStrand)
			    {
				    po.Position = prevExon.CdnaStart;
				    po.Offset = -upDist;
			    }
			    else
			    {
				    po.Position = prevExon.CdnaEnd;
				    po.Offset = upDist;
			    }
		    }
		    else
		    {
			    // distance to downstream exon is the shortest
			    if (onReverseStrand)
			    {
				    po.Position = exon.CdnaEnd;
				    po.Offset = downDist;
			    }
			    else
			    {
				    po.Position = exon.CdnaStart;
				    po.Offset = -downDist;
			    }
		    }
	    }

	    public static string GetTranscriptAllele(string variantAllele, bool onReverseStrand)
	    {
		    return onReverseStrand ? SequenceUtilities.GetReverseComplement(variantAllele) : variantAllele;
	    }

        public static string FormatDnaNotation(string start, string end, string referenceId, string referenceBases, string alternateBases, GenomicChange type,char notationType)
        {
            var sb = new StringBuilder();
            // all start with transcript name & numbering type
            sb.Append(referenceId + ':' + notationType + '.');

            // handle single and multiple positions
            string coordinates = start == end
                ? start
                : start + '_' + end;

            // format rest of string according to type
            // note: inversion and multiple are never assigned as genomic changes
            switch (type)
            {
                case GenomicChange.Deletion:
                    sb.Append(coordinates + "del" + referenceBases);
                    break;
                case GenomicChange.Inversion:
                    sb.Append(coordinates + "inv" + referenceBases);
                    break;
                case GenomicChange.Duplication:
                    sb.Append(coordinates + "dup" + referenceBases);
                    break;
                case GenomicChange.Substitution:
                    sb.Append(start + referenceBases + '>' + alternateBases);
                    break;
                case GenomicChange.DelIns:
                    sb.Append(coordinates + "del" + referenceBases + "ins" + alternateBases); //TODO: change to delins, now use del--ins-- to reduce anavarin differences
                    //sb.Append(coordinates + "delins" + alternateBases);
                    break;
                case GenomicChange.Insertion:
                    sb.Append(coordinates + "ins" + alternateBases);
                    break;

                default:
                    throw new InvalidOperationException("Unhandled genomic change found: " + type);
            }

            return sb.ToString();
        }

    }
}