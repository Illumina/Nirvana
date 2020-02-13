using Genome;
using Moq;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Providers;

namespace UnitTests.SAUtils
{
    public static class SaTestUtils
    {
        public static ISequenceProvider GetSequenceProvider(GenomeAssembly assembly, IChromosome chromosome, int start, string refSequence)
        {
            var seqProvider = new Mock<ISequenceProvider>();
            seqProvider.Setup(x => x.RefNameToChromosome).Returns(ChromosomeUtilities.RefNameToChromosome);
            seqProvider.Setup(x => x.Assembly).Returns(assembly);
            seqProvider.Setup(x => x.Sequence).Returns(new SimpleSequence(refSequence, start - 1));
            return seqProvider.Object;
        }
    }
}