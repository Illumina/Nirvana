using System.Linq;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures.Transcript;

namespace VariantAnnotation.DataStructures.Annotation
{
    public sealed class BreakendTranscriptAnnotation
    {
        #region members

        public readonly string GeneName;

        public readonly bool IsTranscriptSuffix;
        public readonly string HgvsDescription;

        public readonly bool InCodingRegion;
        private readonly string _referenceName;
        private readonly int _codingStart;
        private readonly int _codingEnd;

        public int? Exon;
        public int? Intron;
		public readonly TranscriptDataSource TranscriptDataSource;

        #endregion

        public BreakendTranscriptAnnotation(Transcript.Transcript transcript, int breakendPosition, char isBreakendSuffix)
        {
            var transcriptId = TranscriptUtilities.GetTranscriptId(transcript);
			TranscriptDataSource = transcript.TranscriptSource;

            GeneName = transcript.Gene.Symbol;

            if (transcript.Translation == null || breakendPosition < transcript.Translation.CodingRegion.GenomicStart || breakendPosition > transcript.Translation.CodingRegion.GenomicEnd)
            {
                InCodingRegion = false;
                return;
            }

            InCodingRegion = true;
            var transcriptCdnaLength = transcript.Translation.CodingRegion.CdnaEnd - transcript.Translation.CodingRegion.CdnaStart + 1;
            _referenceName = "chr";
            _codingStart = transcript.Translation.CodingRegion.GenomicStart;
            _codingEnd = transcript.Translation.CodingRegion.GenomicEnd;

            // map cdn position
            var complementaryCdnaPosDescription = MapCdnaPosition(transcript, breakendPosition);

            LocateExonIntron(transcript, breakendPosition);

            var transcriptOrientation = transcript.Gene.OnReverseStrand ? '-' : '+';

            IsTranscriptSuffix = transcriptOrientation == isBreakendSuffix;

            if (IsTranscriptSuffix)
            {
                HgvsDescription = GeneName + "{" + transcriptId + "}" + ":c." + complementaryCdnaPosDescription + "_" +
                                  transcriptCdnaLength;
            }
            else
            {
                HgvsDescription = GeneName + "{" + transcriptId + "}" + ":c." + 1 + "_" + complementaryCdnaPosDescription;
            }
        }

        private void LocateExonIntron(Transcript.Transcript transcript, int pos)
        {
            var exons = transcript.CdnaMaps;
            if (pos < exons.First().GenomicStart || pos > exons.Last().GenomicEnd) return;

            var startIdx = 0;
            while (startIdx < exons.Length)
            {
                if (pos > exons[startIdx].GenomicEnd) startIdx++;
                if (pos < exons[startIdx].GenomicStart)
                {
                    Intron = transcript.Gene.OnReverseStrand ? exons.Length - startIdx : startIdx;
                    return;
                }

                if (pos <= exons[startIdx].GenomicEnd)
                {
                    Exon = transcript.Gene.OnReverseStrand ? exons.Length - startIdx : startIdx + 1;
                    return;
                }
            }
        }

        private static string MapCdnaPosition(Transcript.Transcript transcript, int breakendPosition)
        {
            if (breakendPosition > transcript.End || breakendPosition < transcript.Start) return null;

            var startIdx = 0;
            var endIdx   = transcript.CdnaMaps.Length - 1;

            while (startIdx <= endIdx)
            {
                if (transcript.CdnaMaps[startIdx].GenomicEnd >= breakendPosition) break;
                startIdx++;
            }

            var inExon = breakendPosition >= transcript.CdnaMaps[startIdx].GenomicStart;

            if (inExon)
            {
                var pos = ConvertGenomicPosToCodingPos(breakendPosition, transcript.CdnaMaps[startIdx],
                    transcript.Gene.OnReverseStrand, transcript.Translation.CodingRegion.CdnaStart, transcript.Translation.CodingRegion.CdnaEnd);
                return pos;
            }

            var distance1 = breakendPosition - transcript.CdnaMaps[startIdx - 1].GenomicEnd;
            var distance2 = transcript.CdnaMaps[startIdx].GenomicStart - breakendPosition;

            var distance = distance1;
            var mappedExonIdx = startIdx - 1;
            var mappedExonPos = transcript.CdnaMaps[startIdx - 1].GenomicEnd;
            var orientation = transcript.Gene.OnReverseStrand ? '-' : '+';
            if (distance1 > distance2)
            {
                distance = distance2;
                mappedExonIdx = startIdx;
                mappedExonPos = transcript.CdnaMaps[startIdx].GenomicStart;
                orientation = transcript.Gene.OnReverseStrand ? '+' : '-';
            }

            var exonPos = ConvertGenomicPosToCodingPos(mappedExonPos, transcript.CdnaMaps[mappedExonIdx],
                    transcript.Gene.OnReverseStrand, transcript.Translation.CodingRegion.CdnaStart, transcript.Translation.CodingRegion.CdnaEnd);
            return exonPos + orientation + distance;

        }

        private static string ConvertGenomicPosToCodingPos(int breakendPosition, CdnaCoordinateMap cdnaCoordinateMap,
            bool onReverseStrand, int compDnaCodingStart, int compDnaCodingEnd)
        {
            int cDnaPos;
            if (onReverseStrand)
            {
                cDnaPos = cdnaCoordinateMap.CdnaStart - breakendPosition + cdnaCoordinateMap.GenomicEnd;
            }
            else
            {
                cDnaPos = cdnaCoordinateMap.CdnaEnd + breakendPosition - cdnaCoordinateMap.GenomicEnd;
            }
            if (cDnaPos > compDnaCodingEnd) return "*" + (cDnaPos - compDnaCodingEnd);
            if (cDnaPos < compDnaCodingStart) return (cDnaPos - compDnaCodingStart).ToString();

            return (cDnaPos - compDnaCodingStart + 1).ToString();
        }

        public bool IsTranscriptCodingRegionOverlapped(BreakendTranscriptAnnotation other)
        {
            if (other._referenceName != _referenceName) return false;
            if (other._codingEnd < _codingStart || other._codingStart > _codingEnd) return false;
            return true;
        }

        public bool IsGeneFusion(BreakendTranscriptAnnotation other)
        {
			if (TranscriptDataSource != other.TranscriptDataSource) return false;
            if (!InCodingRegion || !other.InCodingRegion) return false;
            if (GeneName == other.GeneName) return false;
            if (IsTranscriptCodingRegionOverlapped(other)) return false;
            if (IsTranscriptSuffix == other.IsTranscriptSuffix) return false;

            return true;
        }
    }
}