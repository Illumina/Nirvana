using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;

namespace UnitTests.Utilities
{
    public class CacheData
    {
        private readonly ICompressedSequence _compressedSequence;
        private readonly Transcript _transcript;

        /// <summary>
        /// constructor
        /// </summary>
        public CacheData(ICompressedSequence compressedSequence, Transcript transcript)
        {
            _compressedSequence = compressedSequence;
            _transcript = transcript;
        }

        public CodingSequence GetCodingSequence()
        {
            return new CodingSequence(_compressedSequence, _transcript.Translation.CodingRegion.GenomicStart,
                _transcript.Translation.CodingRegion.GenomicEnd, _transcript.CdnaMaps,
                _transcript.Gene.OnReverseStrand, _transcript.StartExonPhase);
        }
    }
}
