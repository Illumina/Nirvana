using System.Text;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.Transcript
{
    public class CodingSequence
    {
        #region members

        private readonly int _start;
        private readonly int _end;
        private readonly CdnaCoordinateMap[] _cdnaMaps;
        private readonly bool _geneOnReverseStrand;
        private readonly int _startExonPhase;
        private readonly ICompressedSequence _sequence;
        private readonly StringBuilder _sb;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public CodingSequence(ICompressedSequence compressedSequence, int start, int end, CdnaCoordinateMap[] cdnaMaps,
            bool geneOnReverseStrand, byte? startExonPhase)
        {
            _start               = start;
            _end                 = end;
            _cdnaMaps            = cdnaMaps;
            _geneOnReverseStrand = geneOnReverseStrand;
            _startExonPhase      = startExonPhase ?? 0;
            _sequence            = compressedSequence;
            _sb                  = new StringBuilder();
        }

        /// <summary>
        /// extracts the coding sequence corresponding to the listed exons
        /// </summary>
        public string Sequence()
        {
            _sb.Clear();

            // account for the exon phase (forward orientation)
            if (_startExonPhase > 0 && !_geneOnReverseStrand) _sb.Append('N', _startExonPhase);

            foreach (var map in _cdnaMaps)
            {
                // handle exons that are entirely in the UTR
                if (map.GenomicEnd < _start || map.GenomicStart > _end) continue;

                int tempBegin = map.GenomicStart;
                int tempEnd = map.GenomicEnd;

                // trim the first and last exons
                if (_start >= tempBegin && _start <= tempEnd) tempBegin = _start;
                if (_end >= tempBegin && _end <= tempEnd) tempEnd = _end;

                _sb.Append(_sequence.Substring(tempBegin - 1, tempEnd - tempBegin + 1));
            }

            // account for the exon phase (reverse orientation)
            if (_startExonPhase > 0 && _geneOnReverseStrand) _sb.Append('N', _startExonPhase);

            return _geneOnReverseStrand ? SequenceUtilities.GetReverseComplement(_sb.ToString()) : _sb.ToString();
        }

        public static int GetCodingSequenceLength(CdnaCoordinateMap[] cdnaMaps, int start, int end, byte startExonPhase)
        {
            int length = startExonPhase;

            foreach (var map in cdnaMaps)
            {
                // handle exons that are entirely in the UTR
                if (map.GenomicEnd < start || map.GenomicStart > end) continue;

                int tempBegin = map.GenomicStart;
                int tempEnd   = map.GenomicEnd;

                // trim the first and last exons
                if (start >= tempBegin && start <= tempEnd) tempBegin = start;
                if (end   >= tempBegin && end   <= tempEnd) tempEnd   = end;

                length += tempEnd - tempBegin + 1;
            }

            return length;
        }
    }
}
