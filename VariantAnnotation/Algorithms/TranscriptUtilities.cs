using System.Collections.Generic;
using System.Text;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.Algorithms
{
    public static class TranscriptUtilities
    {
        /// <summary>
        /// returns the alternate CDS given the reference sequence, the cds coordinates, and the alternate allele.
        /// </summary>
        public static string GetAlternateCds(ICompressedSequence compressedSequence, int cdsBegin, int cdsEnd, string alternateAllele,
            CdnaCoordinateMap[] cdnaMaps, bool onReverseStrand, byte startExonPhase, int cdnaCodingStart)
        {
            var splicedSeq     = GetSplicedSequence(compressedSequence, cdnaMaps, onReverseStrand);
            int numPaddedBases = startExonPhase;

            int shift            = cdnaCodingStart - 1;
            string upstreamSeq   = splicedSeq.Substring(shift, cdsBegin - numPaddedBases - 1);
            string downstreamSeq = splicedSeq.Substring(cdsEnd - numPaddedBases + shift);

            if (alternateAllele == null) alternateAllele = string.Empty;
            var paddedBases = numPaddedBases > 0 ? new string('N', numPaddedBases) : "";

            return paddedBases + upstreamSeq + alternateAllele + downstreamSeq;
        }

        /// <summary>
        /// Retrieves all Exon sequences and concats them together. 
        /// This includes 5' UTR + cDNA + 3' UTR [Transcript.pm:862 spliced_seq]
        /// </summary>
        private static string GetSplicedSequence(ICompressedSequence compressedSequence, CdnaCoordinateMap[] cdnaMaps, bool onReverseStrand)
        {
            var sb = new StringBuilder();

            foreach (var exon in cdnaMaps)
            {
                var exonLength = exon.GenomicEnd - exon.GenomicStart + 1;

                // sanity check: handle the situation where no reference has been provided
                if (compressedSequence == null)
                {
                    sb.Append(new string('N', exonLength));
                    continue;
                }

                sb.Append(compressedSequence.Substring(exon.GenomicStart - 1, exonLength));
            }

            return onReverseStrand ? SequenceUtilities.GetReverseComplement(sb.ToString()) : sb.ToString();
        }

        /// <summary>
        /// returns the total exon length
        /// </summary>
        public static int GetTotalExonLength(CdnaCoordinateMap[] maps)
        {
            int totalExonLength = 0;

            for (int mapIndex = 0; mapIndex < maps.Length; mapIndex++)
            {
                var cdnaMap = maps[mapIndex];
                totalExonLength += cdnaMap.GenomicEnd - cdnaMap.GenomicStart + 1;
            }

            return totalExonLength;
        }

        /// <summary>
        /// calculates the cDNA coordinates given the specified genomic coordinates [Transcript.pm:927 cdna_coding_start]
        /// genomic2pep [TransciptMapper:482]
        /// </summary>
        public static void GetCodingDnaEndpoints(CdnaCoordinateMap[] cdnaMaps, int genomicBegin, int genomicEnd, out int cdnaBegin, out int cdnaEnd)
        {
            // find an overlapping mapper pair
            var coordinateMap = CdnaCoordinateMap.Null();
            bool foundOverlap = false;

            for (int i = 0; i < cdnaMaps.Length; i++)
            {
                coordinateMap = cdnaMaps[i];

                if (genomicEnd >= coordinateMap.GenomicStart &&
                    genomicBegin <= coordinateMap.GenomicEnd)
                {
                    foundOverlap = true;
                    break;
                }
            }

            if (!foundOverlap)
            {
                throw new GeneralException($"Unable to find an overlapping mapping pair for these genomic coordinates: ({genomicBegin}, {genomicEnd})");
            }

            // calculate the cDNA position
            cdnaBegin = coordinateMap.CdnaEnd - (genomicEnd - coordinateMap.GenomicStart);
            cdnaEnd   = coordinateMap.CdnaEnd - (genomicBegin - coordinateMap.GenomicStart);
        }

        /// <summary>
        /// sets both the exon and intron number strings according to which were affected by the variant [BaseTranscriptVariation.pm:474 _exon_intron_number]
        /// </summary>
        public static void ExonIntronNumber(CdnaCoordinateMap[] cdnaMaps, SimpleInterval[] introns, bool onReverseStrand,
            TranscriptAnnotation ta, out string exonNumber, out string intronNumber)
        {
            int exonCount = 0;

            var altAllele       = ta.AlternateAllele;
            var variantInterval = new AnnotationInterval(altAllele.Start, altAllele.End);

            var overlappedExons   = new List<int>();
            var overlappedIntrons = new List<int>();

            var prevExon = CdnaCoordinateMap.Null();

            foreach (var exon in cdnaMaps)
            {
                exonCount++;

                if (variantInterval.Overlaps(exon.GenomicStart, exon.GenomicEnd)) overlappedExons.Add(exonCount);

                if (!prevExon.IsNull)
                {
                    int intronStart = prevExon.GenomicEnd + 1;
                    int intronEnd   = exon.GenomicStart - 1;

                    if (variantInterval.Overlaps(intronStart, intronEnd)) overlappedIntrons.Add(exonCount - 1);
                }

                prevExon = exon;
            }

            exonNumber = GetExonIntronNumber(overlappedExons, cdnaMaps.Length, onReverseStrand);
            intronNumber = introns != null ? GetExonIntronNumber(overlappedIntrons, introns.Length, onReverseStrand) : null;

            if (overlappedExons.Count > 0) ta.HasExonOverlap = true;
        }

        private static string GetExonIntronNumber(List<int> overlappedItems, int totalItems, bool onReverseStrand)
        {
            // sanity check: make sure we have some overlapped items
            if (overlappedItems.Count == 0) return null;

            int firstItem = overlappedItems[0];
            if (onReverseStrand) firstItem = totalItems - firstItem + 1;

            // handle one item
            if (overlappedItems.Count == 1) return firstItem + "/" + totalItems;

            // handle multiple items
            int lastItem = overlappedItems[overlappedItems.Count - 1];

            if (onReverseStrand)
            {
                lastItem = totalItems - lastItem + 1;
                Swap.Int(ref firstItem, ref lastItem);
            }

            return firstItem + "-" + lastItem + "/" + totalItems;
        }

        internal static string GetProteinId(Transcript transcript)
        {
            var translation = transcript.Translation;
            return translation == null
                ? null
                : FormatUtilities.CombineIdAndVersion(translation.ProteinId, translation.ProteinVersion);
        }

        internal static string GetTranscriptId(Transcript transcript)
        {
            return FormatUtilities.CombineIdAndVersion(transcript.Id, transcript.Version);
        }
    }
}
