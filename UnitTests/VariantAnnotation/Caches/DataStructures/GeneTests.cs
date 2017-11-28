using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class GeneTests
    {
        [Fact]
        public void Gene_EndToEnd()
        {
            int expectedStart              = int.MaxValue;
            int expectedEnd                = int.MinValue;
            IChromosome expectedChromosome = new Chromosome("chrBob", "Bob", 1);
            bool expectedReverseStrand     = true;
            var expectedSymbol             = "anavrin";
            var expectedEntrezGeneId       = "7157";
            var expectedEnsemblId          = "ENSG00000141510";
            int expectedHgncId             = int.MaxValue;

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

                using (var reader = new ExtendedBinaryReader(ms))
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
