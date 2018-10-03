using System;
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

        public SimplePosition(IChromosome chromosome, int start, string refAllele, string[] altAlleles)
        {
            Chromosome = chromosome;
            Start      = start;
            RefAllele  = refAllele;
            AltAlleles = altAlleles;
        }

        public static SimplePosition GetSimplePosition(string[] vcfFields, IDictionary<string, IChromosome> refNameToChromosome, bool isRecomposed = false)
        {
            var simplePosition = new SimplePosition(
                ReferenceNameUtilities.GetChromosome(refNameToChromosome, vcfFields[VcfCommon.ChromIndex]),
                Convert.ToInt32(vcfFields[VcfCommon.PosIndex]),
                vcfFields[VcfCommon.RefIndex],
                vcfFields[VcfCommon.AltIndex].OptimizedSplit(','));
            
            
            simplePosition.End = vcfFields[VcfCommon.AltIndex].OptimizedStartsWith('<') || vcfFields[VcfCommon.AltIndex] == "*" ? -1 : simplePosition.Start + simplePosition.RefAllele.Length - 1;
            simplePosition.VcfFields = vcfFields;
            simplePosition.IsRecomposed = isRecomposed;
            simplePosition.IsDecomposed = new bool[simplePosition.AltAlleles.Length]; // fasle by default
            return simplePosition;
        }

        public static SimplePosition GetSimplePosition(string vcfLine,
            IDictionary<string, IChromosome> refNameToChromosome) => vcfLine == null ? null :
            GetSimplePosition(vcfLine.OptimizedSplit('\t'), refNameToChromosome);
    }
}