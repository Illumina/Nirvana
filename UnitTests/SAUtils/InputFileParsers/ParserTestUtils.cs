using System.Collections.Generic;
using Genome;
using UnitTests.TestDataStructures;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace UnitTests.SAUtils.InputFileParsers
{
    public static class ParserTestUtils
    {
        public static ISequenceProvider GetSequenceProvider(int position, string refAllele, char upstreamBase, IDictionary<string, IChromosome> refChromDict)
        {
            var sequence = new SimpleSequence(new string(upstreamBase, VariantUtils.MaxUpstreamLength) + refAllele, position - 1 - VariantUtils.MaxUpstreamLength);

            return new SimpleSequenceProvider(GenomeAssembly.GRCh37, sequence, refChromDict);

        }
    }
}