using CommonUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CodingSequence : ISequence
    {
        private readonly ICodingRegion _codingRegion;
        private readonly ITranscriptRegion[] _regions;
        private readonly bool _geneOnReverseStrand;
        private readonly byte _startExonPhase;
        private readonly ISequence _compressedSequence;
        private string _sequence;

        public CodingSequence(ISequence compressedSequence, ICodingRegion codingRegion, ITranscriptRegion[] regions,
            bool geneOnReverseStrand, byte startExonPhase)
        {
            _codingRegion        = codingRegion;
            _regions             = regions;
            _geneOnReverseStrand = geneOnReverseStrand;
            _startExonPhase      = startExonPhase;
            _compressedSequence  = compressedSequence;
        }

        public string GetCodingSequence()
        {
            var sb = StringBuilderCache.Acquire(Length);

            // account for the exon phase (forward orientation)
            if (_startExonPhase > 0 && !_geneOnReverseStrand) sb.Append('N', _startExonPhase);

            foreach (var region in _regions)
            {
                if (region.Type != TranscriptRegionType.Exon) continue;

                // handle exons that are entirely in the UTR
                if (region.End < _codingRegion.Start || region.Start > _codingRegion.End) continue;

                int tempBegin = region.Start;
                int tempEnd   = region.End;

                // trim the first and last exons
                if (_codingRegion.Start >= tempBegin && _codingRegion.Start <= tempEnd) tempBegin = _codingRegion.Start;
                if (_codingRegion.End   >= tempBegin && _codingRegion.End   <= tempEnd) tempEnd   = _codingRegion.End;

                sb.Append(_compressedSequence.Substring(tempBegin - 1, tempEnd - tempBegin + 1));
            }

            // account for the exon phase (reverse orientation)
            if (_startExonPhase > 0 && _geneOnReverseStrand) sb.Append('N', _startExonPhase);

            var s = StringBuilderCache.GetStringAndRelease(sb);
            return _geneOnReverseStrand ? SequenceUtilities.GetReverseComplement(s) : s;
        }

        public int Length => _codingRegion.Length;

        public string Substring(int offset, int length)
        {
            if (_sequence == null) _sequence = GetCodingSequence();
            return _sequence.Substring(offset, length);
        }
    }
}