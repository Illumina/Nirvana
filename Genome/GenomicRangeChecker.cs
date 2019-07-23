namespace Genome
{
    public sealed class GenomicRangeChecker
    {
        private readonly GenomicRange _genomicRange;
        private bool _reachedLastChromosome;

        public GenomicRangeChecker(GenomicRange genomicRange)
        {
            _genomicRange = genomicRange;
        }

        public bool OutOfRange(IChromosome chromosome, int position)
        {
            if (_genomicRange?.End == null) return false;

            if (!_reachedLastChromosome && chromosome.Equals(_genomicRange.End?.Chromosome)) _reachedLastChromosome = true;

            return _reachedLastChromosome && (position > _genomicRange.End?.Position || !chromosome.Equals(_genomicRange.End?.Chromosome)) ;
        }
    }
}