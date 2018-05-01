using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.Positions;
using Vcf.VariantCreator;

namespace Vcf
{
    public static class VcfReaderUtils
    {
        internal static IPosition ParseVcfLine(string vcfLine, VariantFactory variantFactory, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var simplePosition = SimplePosition.GetSimplePosition(vcfLine, refNameToChromosome);
            return Position.ToPosition(simplePosition, variantFactory);
        }
    }
}