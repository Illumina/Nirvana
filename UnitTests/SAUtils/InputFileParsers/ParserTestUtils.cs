using System.Collections.Generic;
using Amazon.SimpleNotificationService.Model.Internal.MarshallTransformations;
using Genome;
using Moq;
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

        public static IRefMinorProvider GetRefMinorProvider(List<(IChromosome chrom, int position, string globalMinor)> refMinors)
        {
            var refMinorProvider = new Mock<IRefMinorProvider>();
            foreach (var (chrom, position, globalMinor) in refMinors)
            {
                refMinorProvider.Setup(x => x.GetGlobalMajorAllele(chrom, position)).Returns(globalMinor);
            }

            return refMinorProvider.Object;
        }
    }
}