using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.Providers;
using Vcf;
using Vcf.VariantCreator;

namespace UnitTests.TestUtilities
{
    public static class AnnotationUtilities
	{
        internal static Position ParseVcfLine(string vcfLine, IRefMinorProvider refMinorProvider, VariantFactory variantFactory, Dictionary<string, Chromosome> refNameToChromosome)
	    {
	        var simplePosition = SimplePosition.GetSimplePosition(vcfLine, refNameToChromosome);
	        return Position.ToPosition(simplePosition, refMinorProvider, variantFactory);
	    }
    }
}