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
    public sealed class CdnaSequence : ISequence
    {
        private readonly ICodingRegion _codingRegion;
        private readonly ITranscriptRegion[] _regions;
        private readonly IRnaEdit[] _rnaEdits;
        private readonly bool _geneOnReverseStrand;
        private readonly byte _startExonPhase;
        private readonly ISequence _compressedSequence;
        private string _sequence;

        public CdnaSequence(ISequence compressedSequence, ICodingRegion codingRegion, ITranscriptRegion[] regions,
            bool geneOnReverseStrand, byte startExonPhase, IRnaEdit[] rndEdits)
        {
            _codingRegion        = codingRegion;
            _regions             = regions;
            _rnaEdits            = rndEdits;
            _geneOnReverseStrand = geneOnReverseStrand;
            _startExonPhase      = startExonPhase;
            _compressedSequence  = compressedSequence;

            _sequence = GetCdnaSequence();
        }

        public string GetCdnaSequence()
        {
            if (_sequence != null) return _sequence;

            var sb = StringBuilderCache.Acquire(_codingRegion.Length);

            // account for the exon phase (forward orientation)
            if (_startExonPhase > 0 && !_geneOnReverseStrand) sb.Append('N', _startExonPhase);

            foreach (var region in _regions)
            {
                // handle exons that are entirely in the UTR
                //if (region.Type != TranscriptRegionType.Exon || region.End < _codingRegion.Start || region.Start > _codingRegion.End) continue;
                if (region.Type != TranscriptRegionType.Exon) continue;
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
            var editOffset = 0;
            RnaEditUtilities.SetTypesAndSort(_rnaEdits);

            foreach (var rnaEdit in _rnaEdits)
            {
                //if the edits are in utr regions, we can skip them
                var cdnaEditStart = rnaEdit.Start - 1 + editOffset;

                //if (sb.Length <= cdsEditStart) continue;
                
                switch (rnaEdit.Type)
                {
                    case VariantType.SNV:
                        if(cdnaEditStart >= 0 ) sb[cdnaEditStart] = rnaEdit.Bases[0];
                        break;
                    case VariantType.MNV:
                        for (var i = 0; i < rnaEdit.Bases.Length && cdnaEditStart >= 0; i++)
                            sb[cdnaEditStart + i] = rnaEdit.Bases[i];
                        break;
                    case VariantType.insertion:
                        if (cdnaEditStart >= 0) sb.Insert(cdnaEditStart, rnaEdit.Bases);
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
            sb.Append(_compressedSequence.Substring(region.Start - 1, region.End - region.Start + 1));
        }

        public int Length => _sequence?.Length ?? _codingRegion.Length;
        public Band[] CytogeneticBands => null;

        public string Substring(int offset, int length)
        {
            if (_sequence == null) _sequence = GetCdnaSequence();
            return _sequence.Substring(offset, length);
        }
    }
}