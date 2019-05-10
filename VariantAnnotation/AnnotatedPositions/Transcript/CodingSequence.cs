using System.Text;
using ErrorHandling.Exceptions;
using Genome;
using Intervals;
using OptimizedCore;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CodingSequence : ISequence
    {
        private readonly ICodingRegion _codingRegion;
        private readonly ITranscriptRegion[] _regions;
        private readonly IRnaEdit[] _rnaEdits;
        private readonly bool _geneOnReverseStrand;
        private readonly byte _startExonPhase;
        private readonly ISequence _compressedSequence;
        private string _sequence;

        public CodingSequence(ISequence compressedSequence, ICodingRegion codingRegion, ITranscriptRegion[] regions,
            bool geneOnReverseStrand, byte startExonPhase, IRnaEdit[] rndEdits)
        {
            _codingRegion        = codingRegion;
            _regions             = regions;
            _rnaEdits            = rndEdits;
            _geneOnReverseStrand = geneOnReverseStrand;
            _startExonPhase      = startExonPhase;
            _compressedSequence  = compressedSequence;
        }

        public string GetCodingSequence()
        {
            if (_sequence != null) return _sequence;

            var sb = StringBuilderCache.Acquire(Length);

            // account for the exon phase (forward orientation)
            if (_startExonPhase > 0 && !_geneOnReverseStrand) sb.Append('N', _startExonPhase);

            foreach (var region in _regions)
            {
                // handle exons that are entirely in the UTR
                if (region.Type != TranscriptRegionType.Exon || region.End < _codingRegion.Start || region.Start > _codingRegion.End) continue;
                AddCodingRegion(region, sb);
            }

            // account for the exon phase (reverse orientation)
            if (_startExonPhase > 0 && _geneOnReverseStrand) sb.Append('N', _startExonPhase);
            if (_geneOnReverseStrand)
            {
                var revComp = SequenceUtilities.GetReverseComplement(sb.ToString());
                sb.Clear();
                sb.Append(revComp);
            }
            //RNA edits for transcripts on reverse strand come with reversed bases. So, no positional or base adjustment necessary
            // ref: unit test with NM_031947.3, chr5:140682196-140683630
            ApplyRnaEdits(sb);
            _sequence= StringBuilderCache.GetStringAndRelease(sb);

            return _sequence;
        }

        private void ApplyRnaEdits(StringBuilder sb)
        {
            if (_rnaEdits == null) return;
            var codingStart = _codingRegion.CdnaStart;
            var editOffset = 0;
            RnaEditUtilities.SetTypesAndSort(_rnaEdits);

            foreach (var rnaEdit in _rnaEdits)
            {
                //if the edits are in utr regions, we can skip them
                var cdsEditStart = rnaEdit.Start - codingStart + editOffset;

                if (sb.Length <= cdsEditStart) continue;
                
                switch (rnaEdit.Type)
                {
                    case VariantType.SNV:
                        if(cdsEditStart >= 0 ) sb[cdsEditStart] = rnaEdit.Bases[0];
                        break;
                    case VariantType.MNV:
                        for (var i = 0; i < rnaEdit.Bases.Length && cdsEditStart >= 0; i++)
                            sb[cdsEditStart + i] = rnaEdit.Bases[i];
                        break;
                    case VariantType.insertion:
                        if (cdsEditStart >= 0) sb.Insert(cdsEditStart, rnaEdit.Bases);
                        editOffset += rnaEdit.Bases.Length; //account for inserted bases
                        break;
                    case VariantType.deletion:
                        //from the transcripts NM_033089.6 and NM_001317107.1, it seems that deletion edits are
                        //already accounted for in the exons. So, we don't need to delete any more.
                        editOffset -= rnaEdit.End - rnaEdit.Start + 1; //account for deleted bases
                        break;

                    default:
                        throw new UserErrorException("Encountered unknown rnaEdit type:" + rnaEdit.Type);
                }
            }
        }

        private void AddCodingRegion(IInterval region, StringBuilder sb)
        {
            int tempBegin = region.Start;
            int tempEnd   = region.End;

            // trim the first and last exons
            if (_codingRegion.Start >= tempBegin && _codingRegion.Start <= tempEnd) tempBegin = _codingRegion.Start;
            if (_codingRegion.End   >= tempBegin && _codingRegion.End   <= tempEnd) tempEnd   = _codingRegion.End;

            sb.Append(_compressedSequence.Substring(tempBegin - 1, tempEnd - tempBegin + 1));
        }

        public int Length => _codingRegion.Length;

        public string Substring(int offset, int length)
        {
            if (_sequence == null) _sequence = GetCodingSequence();
            return _sequence.Substring(offset, length);
        }
    }
}