using System.Collections.Generic;
using System.IO;
using System.Text;
using Genome;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class GeneTests
    {
        [Fact]
        public void Gene_EndToEnd()
        {
            const int expectedStart           = int.MaxValue;
            const int expectedEnd             = int.MinValue;
            IChromosome expectedChromosome    = new Chromosome("chrBob", "Bob", 1);
            const bool expectedReverseStrand  = true;
            const string expectedSymbol       = "anavrin";
            const string expectedEntrezGeneId = "7157";
            const string expectedEnsemblId    = "ENSG00000141510";
            const int expectedHgncId          = int.MaxValue;

            var indexToChromosome = new Dictionary<ushort, IChromosome>
            {
                [expectedChromosome.Index] = expectedChromosome
            };

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var gene = new Gene(expectedChromosome, expectedStart, expectedEnd, expectedReverseStrand,
                expectedSymbol, expectedHgncId, CompactId.Convert(expectedEntrezGeneId),
                CompactId.Convert(expectedEnsemblId));
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            IGene observedGene;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    gene.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new BufferedBinaryReader(ms))
                {
                    observedGene = Gene.Read(reader, indexToChromosome);
                }
            }

            Assert.NotNull(observedGene);
            Assert.Equal(expectedStart,            observedGene.Start);
            Assert.Equal(expectedEnd,              observedGene.End);
            Assert.Equal(expectedChromosome.Index, observedGene.Chromosome.Index);
            Assert.Equal(expectedReverseStrand,    observedGene.OnReverseStrand);
            Assert.Equal(expectedSymbol,           observedGene.Symbol);
            Assert.Equal(expectedEntrezGeneId,     observedGene.EntrezGeneId.ToString());
            Assert.Equal(expectedEnsemblId,        observedGene.EnsemblId.ToString());
            Assert.Equal(expectedHgncId,           observedGene.HgncId);
        }
    }
}
