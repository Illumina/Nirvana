using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Vcf
{
    public sealed class SimplePosition : ISimplePosition
    {
        public int Start { get; }
        public int End { get; private set; }
        public IChromosome Chromosome { get; }
        public string RefAllele { get; }
        public string[] AltAlleles { get; }
        public string[] VcfFields { get; private set; }
        public bool[] IsDecomposed { get; private set; }
        public bool IsRecomposed { get; private set; }
        public string[] Vids { get; private set; }
        public List<string>[] LinkedVids { get; private set; }

        private SimplePosition(IChromosome chromosome, int start, string refAllele, string[] altAlleles)
        {
            Chromosome = chromosome;
            Start      = start;
            RefAllele  = refAllele;
            AltAlleles = altAlleles;
        }

        public static SimplePosition GetSimplePosition(string[] vcfFields, IVcfFilter vcfFilter, IDictionary<string, IChromosome> refNameToChromosome, bool isRecomposed = false)
        {
            var simplePosition = new SimplePosition(
                ReferenceNameUtilities.GetChromosome(refNameToChromosome, vcfFields[VcfCommon.ChromIndex]),
                int.Parse(vcfFields[VcfCommon.PosIndex]),
                vcfFields[VcfCommon.RefIndex],
                vcfFields[VcfCommon.AltIndex].OptimizedSplit(','));
            
            if (vcfFilter.PassedTheEnd(simplePosition.Chromosome, simplePosition.Start)) return null;
            
            simplePosition.End = vcfFields[VcfCommon.AltIndex].OptimizedStartsWith('<') || vcfFields[VcfCommon.AltIndex] == "*" ? -1 : simplePosition.Start + simplePosition.RefAllele.Length - 1;
            simplePosition.VcfFields = vcfFields;
            simplePosition.IsRecomposed = isRecomposed;
            simplePosition.IsDecomposed = new bool[simplePosition.AltAlleles.Length]; // false by default
            simplePosition.Vids = new string[simplePosition.AltAlleles.Length];
            simplePosition.LinkedVids = new List<string>[simplePosition.AltAlleles.Length];
            return simplePosition;
        }

        public static SimplePosition GetSimplePosition(string vcfLine, IVcfFilter vcfFilter,
            IDictionary<string, IChromosome> refNameToChromosome) => vcfLine == null ? null :
            GetSimplePosition(vcfLine.OptimizedSplit('\t'), vcfFilter, refNameToChromosome);
    }
}