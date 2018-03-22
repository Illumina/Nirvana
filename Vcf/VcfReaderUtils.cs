using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using Vcf.VariantCreator;

namespace Vcf
{
    public static class VcfReaderUtils
    {
        internal static IPosition ParseVcfLine(string vcfLine, VariantFactory variantFactory, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var simplePosition = SimplePosition.GetSimplePosition(vcfLine, refNameToChromosome);
            return Position.CreatFromSimplePosition(simplePosition, variantFactory);
        }
    }
}