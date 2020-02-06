using System.Collections.Generic;
using Genome;
using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.Interface.Providers;

namespace UnitTests.SAUtils
{
    public static class SaTestUtils
    {
        public static ISequenceProvider GetSequenceProvider(GenomeAssembly assembly, IChromosome chromosome, int start, string refSequence)
        {
            var seqProvider = new Mock<ISequenceProvider>();
            if (chromosome.EnsemblName == "X" || chromosome.EnsemblName == "Y")
                seqProvider.Setup(x => x.RefNameToChromosome).Returns(new Dictionary<string, IChromosome> { { "X", new Chromosome("chrX", "X", 1) }, { "Y", new Chromosome("chrY", "Y", 2) } });
            else
                seqProvider.Setup(x => x.RefNameToChromosome).Returns(new Dictionary<string, IChromosome> { { chromosome.EnsemblName, chromosome } });
            seqProvider.Setup(x => x.Assembly).Returns(assembly);
            seqProvider.Setup(x => x.Sequence).Returns(new SimpleSequence(refSequence, start - 1));
            return seqProvider.Object;
        }

    }
}