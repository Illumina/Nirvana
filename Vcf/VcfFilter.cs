using System.IO;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;

namespace Vcf
{
    public sealed class VcfFilter : IVcfFilter
    {
        private readonly GenomicRange _genomicRange;
        private readonly GenomicRangeChecker _genomicRangeChecker;
        internal string BufferedLine;

        public VcfFilter(GenomicRange genomicRange)
        {
            _genomicRange = genomicRange;
            _genomicRangeChecker = new GenomicRangeChecker(genomicRange);
        }

        public void FastForward(StreamReader reader)
        {
            string line;
             while ((line = reader.ReadLine()) != null)
             {
                if (line.StartsWith('#')) continue;

                var fields = line.OptimizedSplit('\t');
                string chrName = fields[VcfCommon.ChromIndex];
                if (chrName != _genomicRange.Start.Chromosome.UcscName && chrName != _genomicRange.Start.Chromosome.EnsemblName) continue;

                int position = int.Parse(fields[VcfCommon.PosIndex]);
                if (position < _genomicRange.Start.Position) continue;

                BufferedLine = line;
                return;
             }
        }

        public string GetNextLine(StreamReader reader)
        {
            if (BufferedLine == null)
            {
                return reader.ReadLine();
            }
            string bufferedLine = BufferedLine;
            BufferedLine = null;

            return bufferedLine;
        }

        public bool PassedTheEnd(IChromosome chromosome, int position) => _genomicRangeChecker.OutOfRange(chromosome, position);
    }
}