using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CodingSequence:ISequence
    {
        #region members

        private readonly int _start;
        private readonly int _end;
        private readonly ICdnaCoordinateMap[] _cdnaMaps;
        private readonly bool _geneOnReverseStrand;
        private readonly int _startExonPhase;
        private readonly ISequence _compressedSequence;
        private string _sequence;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public CodingSequence(ISequence compressedSequence, int start, int end, ICdnaCoordinateMap[] cdnaMaps,
            bool geneOnReverseStrand, byte? startExonPhase)
        {
            _start = start;
            _end = end;
            _cdnaMaps = cdnaMaps;
            _geneOnReverseStrand = geneOnReverseStrand;
            _startExonPhase = startExonPhase ?? 0;            
           // _sequence = GetCodingSequence(compressedSequence);
            _compressedSequence = compressedSequence;
        }

        /// <summary>
        /// extracts the coding sequence corresponding to the listed exons
        /// </summary>
        private string GetCodingSequence(ISequence genomicSequence)
        {
            var sb = new StringBuilder();

            // account for the exon phase (forward orientation)
            if (_startExonPhase > 0 && !_geneOnReverseStrand) sb.Append('N', _startExonPhase);

            foreach (var map in _cdnaMaps)
            {
                // handle exons that are entirely in the UTR
                if (map.End < _start || map.Start > _end) continue;

                int tempBegin = map.Start;
                int tempEnd = map.End;

                // trim the first and last exons
                if (_start >= tempBegin && _start <= tempEnd) tempBegin = _start;
                if (_end >= tempBegin && _end <= tempEnd) tempEnd = _end;

                sb.Append(genomicSequence.Substring(tempBegin - 1, tempEnd - tempBegin + 1));
            }

            // account for the exon phase (reverse orientation)
            if (_startExonPhase > 0 && _geneOnReverseStrand) sb.Append('N', _startExonPhase);

            return _geneOnReverseStrand ? SequenceUtilities.GetReverseComplement(sb.ToString()) : sb.ToString();
        }

        public static int GetCodingSequenceLength(ICdnaCoordinateMap[] cdnaMaps, int start, int end, byte startExonPhase)
        {
            int length = startExonPhase;

            foreach (var map in cdnaMaps)
            {
                // handle exons that are entirely in the UTR
                if (map.End < start || map.Start > end) continue;

                int tempBegin = map.Start;
                int tempEnd = map.End;

                // trim the first and last exons
                if (start >= tempBegin && start <= tempEnd) tempBegin = start;
                if (end >= tempBegin && end <= tempEnd) tempEnd = end;

                length += tempEnd - tempBegin + 1;
            }

            return length;
        }

        public int Length
        {
            get
            {
                if (_sequence == null) _sequence = GetCodingSequence(_compressedSequence);
                return _sequence.Length;
            }
        }
        public string Substring(int offset, int length)
        {
            if(_sequence ==null) _sequence = GetCodingSequence(_compressedSequence);
            return _sequence.Substring(offset, length);
        }
    }
}