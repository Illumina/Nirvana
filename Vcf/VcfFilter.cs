using System.IO;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;

namespace Vcf
{
    public sealed class VcfFilter : IVcfFilter
    {
        private readonly IChromosomeInterval _annotationInterval;
        internal string BufferedLine;

        public VcfFilter(IChromosomeInterval annotationInterval)
        {
            _annotationInterval = annotationInterval;
        }


        public void FastForward(StreamReader reader)
        {
            string line;
             while ((line = reader.ReadLine()) != null)
             {
                if (line.StartsWith('#')) continue;

                var fields = line.OptimizedSplit('\t');
                string chrName = fields[VcfCommon.ChromIndex];
                if (chrName != _annotationInterval.Chromosome.EnsemblName && chrName != _annotationInterval.Chromosome.UcscName) continue;

                int position = int.Parse(line.OptimizedSplit('\t')[VcfCommon.PosIndex]);
                if (position < _annotationInterval.Start) continue;

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

        public bool PassedTheEnd(IChromosome chromosome, int position) => position > _annotationInterval.End || !_annotationInterval.Chromosome.Equals(chromosome);
    }
}