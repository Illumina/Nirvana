using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CodingSequence : ISequence
    {
        private readonly string _sequence;

        public CodingSequence(ISequence compressedSequence, ICodingRegion codingRegion, ITranscriptRegion[] regions,
                              bool onReverseStrand, byte startExonPhase, IRnaEdit[] rnaEdits)
        {
            string cdnaSequence = 
                new CdnaSequence(compressedSequence, codingRegion, regions, onReverseStrand, rnaEdits)
                    .GetCdnaSequence();
            int cdsLen = codingRegion.CdnaEnd - codingRegion.CdnaStart + 1;
            
            _sequence = new string('N', startExonPhase) + cdnaSequence.Substring(codingRegion.CdnaStart - 1, cdsLen);
        }

        public string GetCodingSequence()               => _sequence;
        public int    Length                            => _sequence.Length;
        public Band[] CytogeneticBands                  => null;
        public string Substring(int offset, int length) => _sequence.Substring(offset, length);
    }
}