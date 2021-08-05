using System;
using Genome;
using Intervals;
using OptimizedCore;
using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsUtilities
    {
        public static void ShiftAndRotateAlleles(ref int start, ref string refAminoAcids, ref string altAminoAcids,
            string peptideSeq)
        {
            (start, refAminoAcids, altAminoAcids) = BiDirectionalTrimmer.Trim(start, refAminoAcids, altAminoAcids);
            (start, refAminoAcids, altAminoAcids) = Rotate3Prime(refAminoAcids, altAminoAcids, start, peptideSeq);
        }

        internal static (int Start, string RefAminoAcids, string AltAminoAcids) Rotate3Prime(string refAminoAcids,
            string altAminoAcids, int start, string peptides)
        {
            if (!(string.IsNullOrEmpty(refAminoAcids) || string.IsNullOrEmpty(altAminoAcids)))
                return (start, refAminoAcids, altAminoAcids);

            bool isInsertion = !string.IsNullOrEmpty(altAminoAcids);

            // ReSharper disable once PossibleNullReferenceException
            int end = start + refAminoAcids.Length - 1;

            // for insertion, the reference bases will be empty string. The shift should happen on the alternate allele
            string rotatingPeptides = isInsertion ? altAminoAcids : refAminoAcids;
            int    numBases         = rotatingPeptides.Length;

            string downstreamPeptides = peptides.Length >= end ? peptides.Substring(end) : null;
            string combinedSequence   = rotatingPeptides + downstreamPeptides;

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
            else refAminoAcids             = rotatingPeptides;

            return (start, refAminoAcids, altAminoAcids);
        }

        /// <summary>
        /// returns true if this insertion has the same amino acids preceding it [TranscriptVariationAllele.pm:1494 _check_for_peptide_duplication]
        /// </summary>
        public static bool IsAminoAcidDuplicate(int start, string altAminoAcids, string transcriptPeptides)
        {
            if (altAminoAcids == null || transcriptPeptides == null) return false;

            int testAminoAcidPos = start - altAminoAcids.Length - 1;
            if (testAminoAcidPos < 0) return false;

            string precedingAminoAcids = testAminoAcidPos + altAminoAcids.Length <= transcriptPeptides.Length
                ? transcriptPeptides.Substring(testAminoAcidPos, altAminoAcids.Length)
                : "";

            return precedingAminoAcids == altAminoAcids;
        }

        /// <summary>
        /// returns the number of amino acids until the next stop codon is encountered [TranscriptVariationAllele.pm:1531 _stop_loss_extra_AA]
        /// </summary>
        public static int GetNumAminoAcidsUntilStopCodon(string altCds, string peptideSeq, int refVarPos,
            bool isFrameshift)
        {
            int numExtraAminoAcids = -1;
            int refLen             = peptideSeq.Length;

            // find the number of residues that are translated until a termination codon is encountered
            int terPos = altCds.IndexOf('*');
            if (terPos != -1)
            {
                numExtraAminoAcids = terPos + 1 - (isFrameshift ? refVarPos : refLen + 1);
            }

            // A special case is if the first aa is a stop codon => don't display the number of residues until the stop codon
            return numExtraAminoAcids > 0 ? numExtraAminoAcids : -1;
        }

        public static (int Start, char RefAminoAcid, char AltAminoAcid) GetChangesAfterFrameshift(int start,
            string peptideSeq, string altPeptideSeq)
        {
            start = Math.Min(start, peptideSeq.Length);

            // for deletions at the end of peptide sequence
            if (start > altPeptideSeq.Length) return (start, peptideSeq[start - 1], 'X');

            string refPeptideSeq = peptideSeq + "*";
            char   refAminoAcid  = refPeptideSeq[start - 1];
            char   altAminoAcid  = altPeptideSeq[start - 1];

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
            string transcriptAltAllele, ITranscript transcript, AminoAcid aminoAcids, int aaStart)
        {
            string altCds = TranscriptUtilities.GetAlternateCds(refSequence, cdsBegin, cdsEnd, transcriptAltAllele,
                transcript.TranscriptRegions, transcript.Gene.OnReverseStrand, transcript.StartExonPhase,
                transcript.Translation.CodingRegion.CdnaStart);
            
            return aminoAcids.TranslateBases(altCds, transcript.AminoAcidEdits, aaStart, true);
        }

        public static PositionOffset GetCdnaPositionOffset(ITranscript transcript, int position, int regionIndex,
            bool isRegionStart)
        {
            if (!transcript.Overlaps(position, position)) return null;

            var region = transcript.TranscriptRegions[regionIndex];
            int codingRegionStart = transcript.Translation?.CodingRegion.CdnaStart ?? -1;
            int codingRegionEnd = transcript.Translation?.CodingRegion.CdnaEnd ?? -1;
            var po = GetPositionAndOffset(position, region, transcript.Gene.OnReverseStrand, isRegionStart);
            if (po.Position == -1) return null;

            var    cdnaCoord = GetCdnaCoord(po.Position, codingRegionStart, codingRegionEnd);
            string offset    = po.Offset == 0 ? "" : po.Offset.ToString("+0;-0;+0");
            string value     = cdnaCoord.CdnaCoord + offset;

            return new PositionOffset(po.Position, po.Offset, value, cdnaCoord.HasStopCodonNotation);
        }

        private static (int Position, int Offset) GetPositionAndOffset(int position, ITranscriptRegion region,
            bool onReverseStrand, bool isRegionStart)
        {
            int cdsPos = -1;
            int offset = -1;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (region.Type)
            {
                case TranscriptRegionType.Exon:
                    cdsPos = region.CdnaStart + (onReverseStrand ? region.End - position : position - region.Start);
                    offset = 0;
                    break;
                case TranscriptRegionType.Gap:
                    (cdsPos, offset) = GetGapPositionAndOffset(region, isRegionStart);
                    break;
                case TranscriptRegionType.Intron:
                    (cdsPos, offset) = GetIntronPositionAndOffset(position, region, onReverseStrand);
                    break;
            }

            return (cdsPos, offset);
        }

        private static (int Position, int Offset) GetIntronPositionAndOffset(int position, ITranscriptRegion region,
            bool onReverseStrand)
        {
            int leftDist  = position   - region.Start + 1;
            int rightDist = region.End - position     + 1;

            int offset = Math.Min(leftDist, rightDist);
            if (!onReverseStrand && rightDist < leftDist || onReverseStrand && rightDist > leftDist) offset = -offset;

            // cDNA position truth table
            //
            //          forward     reverse
            //       -------------------------
            // L < R | CdnaStart | CdnaEnd   |
            // L = R | CdnaStart | CdnaStart |
            // L > R | CdnaEnd   | CdnaStart |
            //       -------------------------

            int cdnaPosition = leftDist < rightDist && onReverseStrand || leftDist > rightDist && !onReverseStrand
                ? region.CdnaEnd
                : region.CdnaStart;

            return (cdnaPosition, offset);
        }

        private static (int Position, int Offset)
            GetGapPositionAndOffset(ITranscriptRegion region, bool isRegionStart) =>
            isRegionStart ? (region.CdnaEnd, 0) : (region.CdnaStart, 0);

        private static (string CdnaCoord, bool HasStopCodonNotation) GetCdnaCoord(int position, int codingRegionStart,
            int codingRegionEnd)
        {
            string cdnaCoord            = null;
            var    hasStopCodonNotation = false;

            if (codingRegionEnd != -1)
            {
                if (position > codingRegionEnd)
                {
                    cdnaCoord            = "*" + (position - codingRegionEnd);
                    hasStopCodonNotation = true;
                }
            }

            if (!hasStopCodonNotation && codingRegionStart != -1)
            {
                cdnaCoord = (position + (position >= codingRegionStart ? 1 : 0) - codingRegionStart).ToString();
            }

            if (cdnaCoord == null) cdnaCoord = position.ToString();
            return (cdnaCoord, hasStopCodonNotation);
        }

        public static string AdjustTranscriptRefAllele(string transcriptRefAllele, int coveredCdnaStart,
            int coveredCdnaEnd, ISequence cdnaSequence)
        {
            if (coveredCdnaStart == -1 || coveredCdnaEnd == -1 || cdnaSequence == null) return transcriptRefAllele;
            
            return coveredCdnaEnd < coveredCdnaStart
                ? string.Empty
                : cdnaSequence.Substring(coveredCdnaStart - 1, coveredCdnaEnd - coveredCdnaStart + 1);
        }

        public static string GetTranscriptAllele(string variantAllele, bool onReverseStrand) =>
            onReverseStrand ? SequenceUtilities.GetReverseComplement(variantAllele) : variantAllele;

        public static string FormatDnaNotation(string start, string end, string referenceId, string referenceBases,
            string alternateBases, GenomicChange type, char notationType)
        {
            var sb = StringBuilderCache.Acquire();

            // all start with transcript name & numbering type
            sb.Append(referenceId + ':' + notationType + '.');

            // handle single and multiple positions
            string coordinates = start == end
                ? start
                : start + '_' + end;

            // format rest of string according to type
            // note: inversion and multiple are never assigned as genomic changes
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case GenomicChange.Deletion:
                    sb.Append(coordinates + "del");
                    break;
                case GenomicChange.Inversion:
                    sb.Append(coordinates + "inv");
                    break;
                case GenomicChange.Duplication:
                    sb.Append(coordinates + "dup");
                    break;
                case GenomicChange.Substitution:
                    if (referenceBases == alternateBases)
                    {
                        sb.Append(start + '=');
                    }
                    else
                    {
                        sb.Append(start + referenceBases + '>' + alternateBases);
                    }

                    break;
                case GenomicChange.DelIns:
                    // NOTE: change to delins, now use del--ins-- to reduce Anavrin differences
                    sb.Append(coordinates + "delins" + alternateBases);
                    break;
                case GenomicChange.Insertion:
                    sb.Append(coordinates + "ins" + alternateBases);
                    break;

                default:
                    throw new InvalidOperationException("Unhandled genomic change found: " + type);
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public static bool IsDuplicateWithinInterval(ISequence refSequence, ISimpleVariant variant, IInterval interval,
            bool onReverseStrand)
        {
            if (variant.Type != VariantType.insertion) return false;

            int    altAlleleLen = variant.AltAllele.Length;
            string compareRegion;

            if (onReverseStrand)
            {
                if (variant.End + altAlleleLen > interval.End) return false;
                compareRegion = refSequence.Substring(variant.Start - 1, altAlleleLen);
            }
            else
            {
                if (variant.Start - altAlleleLen < interval.Start) return false;
                compareRegion = refSequence.Substring(variant.End - altAlleleLen, altAlleleLen);
            }

            return compareRegion == variant.AltAllele;
        }
    }
}