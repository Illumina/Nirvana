using Cache.Data;
using VariantAnnotation.Caches.DataStructures;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class MappedPositionUtilities
    {
        public static (int Index, TranscriptRegion Region) FindRegion(TranscriptRegion[] regions,
            int variantPosition)
        {
            int index  = regions.BinarySearch(variantPosition);
            var region = index < 0 ? null : regions[index];
            return (index, region);
        }

        public static (int CdnaStart, int CdnaEnd) GetCdnaPositions(TranscriptRegion startRegion,
            TranscriptRegion endRegion, int start, int end, bool onReverseStrand)
        {
            int cdnaStart = GetCdnaPosition(startRegion, start, onReverseStrand);
            int cdnaEnd   = GetCdnaPosition(endRegion,   end,   onReverseStrand);
            return (cdnaStart, cdnaEnd);
        }

        public static (int CdnaStart, int CdnaEnd) GetInsertionCdnaPositions(TranscriptRegion startRegion,
            TranscriptRegion endRegion, int start, int end, bool onReverseStrand)
        {
            int cdnaStart, cdnaEnd;

            if (onReverseStrand)
            {
                cdnaStart = GetCdnaPosition(startRegion, start, true);
                if (cdnaStart != -1) return (cdnaStart, cdnaStart + 1);

                cdnaEnd = GetCdnaPosition(endRegion, end, true);
                return cdnaEnd != -1 ? (cdnaEnd - 1, cdnaEnd) : (-1, -1);
            }

            cdnaEnd = GetCdnaPosition(endRegion, end, false);
            if (cdnaEnd != -1) return (cdnaEnd + 1, cdnaEnd);

            cdnaStart = GetCdnaPosition(startRegion, start, false);
            return cdnaStart != -1 ? (cdnaStart, cdnaStart - 1) : (-1, -1);
        }

        public static int GetCdnaPosition(TranscriptRegion region, int variantPosition, bool onReverseStrand)
        {
            if (region == null || region.Type != TranscriptRegionType.Exon) return -1;

            if (region.CigarOps != null) return GetCigarCdnaPosition(region, variantPosition, onReverseStrand);

            return onReverseStrand
                ? region.End      - variantPosition + region.CdnaStart
                : variantPosition - region.Start    + region.CdnaStart;
        }

        public static int GetCigarCdnaPosition(TranscriptRegion region, int position, bool onReverseStrand)
        {
            // start just before the exon
            int  genomicPosition = onReverseStrand ? region.End + 1 : region.Start - 1;
            int  cdnaPosition    = region.CdnaStart - 1;

            foreach (CigarOp cigarOp in region.CigarOps!)
            {
                (int deltaGenome, int deltaCdna) = GetCigarOpDelta(cigarOp, onReverseStrand);
                int nextGenomicPosition = genomicPosition + deltaGenome;

                bool lastCigarOp = onReverseStrand ? nextGenomicPosition <= position : nextGenomicPosition >= position;

                if (lastCigarOp)
                {
                    int remaining = onReverseStrand ? genomicPosition - position : position - genomicPosition;
                    (deltaGenome, deltaCdna) = GetConstrainedCigarOpDelta(cigarOp.Type, remaining, onReverseStrand);
                }

                genomicPosition += deltaGenome;
                cdnaPosition    += deltaCdna;

                if (lastCigarOp) break;
            }

            return cdnaPosition;
        }

        private static (int DeltaGenome, int DeltaCdna) GetCigarOpDelta(CigarOp cigarOp, bool onReverseStrand)
        {
            int deltaGenome = onReverseStrand ? -cigarOp.Length : cigarOp.Length;
            return cigarOp.Type switch
                   {
                       CigarType.Deletion  => (deltaGenome, 0),
                       CigarType.Insertion => (0, cigarOp.Length),
                       _                   => (deltaGenome, cigarOp.Length) // match
                   };
        }
        
        private static (int DeltaGenome, int DeltaCdna) GetConstrainedCigarOpDelta(CigarType cigarType, int remainingBases, bool onReverseStrand)
        {
            int deltaGenome = onReverseStrand ? -remainingBases : remainingBases;
            return cigarType switch
                   {
                       CigarType.Deletion  => (deltaGenome, 0),
                       CigarType.Insertion => (0, remainingBases),
                       _                   => (deltaGenome, remainingBases) // match
                   };
        }

        /// <summary>
        /// Assuming at least one cDNA coordinate overlaps with an exon, the covered cDNA coordinates represent
        /// the coordinates actually covered by the variant.
        /// </summary>
        public static (int Start, int End) GetCoveredCdnaPositions(this TranscriptRegion[] regions, int cdnaStart, int startRegionIndex,
            int cdnaEnd, int endRegionIndex, bool onReverseStrand)
        {
            // exon case
            if (cdnaStart != -1 && cdnaEnd != -1) return (cdnaStart, cdnaEnd);

            if (onReverseStrand) (startRegionIndex, endRegionIndex) = (endRegionIndex, startRegionIndex);

            var startRegion = regions.GetCoveredRegion(startRegionIndex);
            var endRegion   = regions.GetCoveredRegion(endRegionIndex);

            if (startRegion.Type != TranscriptRegionType.Exon && endRegion.Type != TranscriptRegionType.Exon)
                return (-1, -1);

            int codingEnd = onReverseStrand ? regions[0].CdnaEnd : regions[regions.Length - 1].CdnaEnd;

            cdnaStart = GetCoveredCdnaPosition(true, cdnaStart, startRegion, startRegionIndex, onReverseStrand, codingEnd);
            cdnaEnd   = GetCoveredCdnaPosition(false, cdnaEnd, endRegion, endRegionIndex, onReverseStrand, codingEnd);

            return cdnaStart < cdnaEnd ? (cdnaStart, cdnaEnd) : (cdnaEnd, cdnaStart);
        }

        private static TranscriptRegion GetCoveredRegion(this TranscriptRegion[] regions, int regionIndex)
        {
            if (regionIndex == -1) return regions[0];
            return regionIndex == ~regions.Length ? regions[regions.Length - 1] : regions[regionIndex];
        }

        private static int GetCoveredCdnaPosition(bool isStart, int cdnaPosition, TranscriptRegion region, int regionIndex, bool onReverseStrand, int codingEnd)
        {
            if (cdnaPosition >= 0) return cdnaPosition;

            // start before transcript
            if (regionIndex == -1) return onReverseStrand ? codingEnd : 1;

            // end after transcript
            if (regionIndex < -1) return onReverseStrand ? 1 : codingEnd;

            // intron
            return isStart ? region.CdnaEnd : region.CdnaStart;
        }

        public static (int CdsStart, int CdsEnd, int ProteinStart, int ProteinEnd) GetCoveredCdsAndProteinPositions(
            int coveredCdnaStart, int coveredCdnaEnd, CodingRegion codingRegion)
        {
            if (codingRegion     == null                  ||
                coveredCdnaEnd   < codingRegion.CdnaStart ||
                coveredCdnaStart > codingRegion.CdnaEnd   ||
                coveredCdnaStart == -1 && coveredCdnaEnd == -1) return (-1, -1, -1, -1);

            if (coveredCdnaStart < codingRegion.CdnaStart) coveredCdnaStart = codingRegion.CdnaStart;
            if (coveredCdnaEnd   > codingRegion.CdnaEnd)   coveredCdnaEnd   = codingRegion.CdnaEnd;

            int offset = codingRegion.CdsOffset - codingRegion.CdnaStart + 1;
            int start  = coveredCdnaStart + offset;
            int end    = coveredCdnaEnd   + offset;

            return (start, end, GetProteinPosition(start), GetProteinPosition(end));
        }

        public static int GetProteinPosition(int cdsPosition)
        {
            if (cdsPosition == -1) return -1;
            return (cdsPosition + 2) / 3;
        }

        public static (int CdsStart, int CdsEnd) GetCdsPositions(CodingRegion codingRegion, int cdnaStart,
            int cdnaEnd, bool isInsertion)
        {
            int cdsStart = GetCdsPosition(codingRegion, cdnaStart);
            int cdsEnd   = GetCdsPosition(codingRegion, cdnaEnd);

            // silence CDS for insertions that occur just after the coding region
            if (isInsertion && codingRegion != null && (cdnaEnd == codingRegion.CdnaEnd || cdnaStart == codingRegion.CdnaStart))
            {
                cdsStart = -1;
                cdsEnd   = -1;
            }

            return (cdsStart, cdsEnd);
        }

        private static int GetCdsPosition(CodingRegion codingRegion, int cdnaPosition)
        {
            if (codingRegion == null || cdnaPosition < codingRegion.CdnaStart ||
                cdnaPosition > codingRegion.CdnaEnd) return -1;
            return cdnaPosition - codingRegion.CdnaStart + codingRegion.CdsOffset + 1;
        }

        // this is used to get CDS coordinates past the last CDS position
        public static int GetExtendedCdsPosition(int cdnaStart, int cdnaPosition, ushort cdsOffset)
        {
            if (cdnaPosition < cdnaStart) return -1;
            return cdnaPosition - cdnaStart + cdsOffset + 1;
        }
    }
}