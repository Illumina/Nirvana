using System.Text;
using ErrorHandling.Exceptions;
using Genome;
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
        private readonly bool _onReverseStrand;
        private readonly ISequence _compressedSequence;
        private string _sequence;

        public CdnaSequence(ISequence compressedSequence, ICodingRegion codingRegion, ITranscriptRegion[] regions,
            bool onReverseStrand, IRnaEdit[] rndEdits)
        {
            _codingRegion       = codingRegion;
            _regions            = regions;
            _rnaEdits           = rndEdits;
            _onReverseStrand    = onReverseStrand;
            _compressedSequence = compressedSequence;

            _sequence = GetCdnaSequence();
        }

        public string GetCdnaSequence()
        {
            if (_sequence != null) return _sequence;

            var sb = StringBuilderCache.Acquire();
            
            foreach (var region in _regions)
            {
                if (region.Type != TranscriptRegionType.Exon) continue;
                sb.Append(_compressedSequence.Substring(region.Start - 1, region.End - region.Start + 1));
            }

            if (_onReverseStrand)
            {
                string reverseComplement = SequenceUtilities.GetReverseComplement(sb.ToString());
                sb.Clear();
                sb.Append(reverseComplement);
            }

            ApplyRnaEdits(sb);

            _sequence = StringBuilderCache.GetStringAndRelease(sb);
            return _sequence;
        }

        private void ApplyRnaEdits(StringBuilder sb)
        {
            if (_rnaEdits == null) return;
            var editOffset = 0;
            RnaEditUtilities.SetTypesAndSort(_rnaEdits);

            foreach (var rnaEdit in _rnaEdits)
            {
                int cdnaEditStart = rnaEdit.Start - 1 + editOffset;
                
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
                        editOffset += rnaEdit.Bases.Length;
                        break;
                    
                    case VariantType.deletion:
                        editOffset -= rnaEdit.End - rnaEdit.Start + 1;
                        break;

                    default:
                        throw new UserErrorException("Encountered unknown rnaEdit type:" + rnaEdit.Type);
                }
            }
        }
        
        public int Length => _sequence?.Length ?? _codingRegion?.Length ?? 0;
        public Band[] CytogeneticBands => null;

        public string Substring(int offset, int length)
        {
            if (_sequence == null) _sequence = GetCdnaSequence();
            return _sequence.Substring(offset, length);
        }
    }
}