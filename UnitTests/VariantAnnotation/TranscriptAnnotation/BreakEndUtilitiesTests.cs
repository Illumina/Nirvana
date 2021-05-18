using System.IO;
using Genome;
using UnitTests.TestUtilities;
using VariantAnnotation.GeneFusions.Calling;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class BreakEndUtilitiesTests
    {
        [Theory]
        [InlineData(28722335, "T", "[3:115024109[T",             true,  "3",                 115024109, false)]
        [InlineData(31410878, "C", "]6:42248252]C",              true,  "6",                 42248252,  true)]
        [InlineData(31561816, "C", "CGATCTCAT[6:41297838[",      false, "6",                 41297838,  false)]
        [InlineData(84461562, "A", "A]8:100990100]",             false, "8",                 100990100, true)]
        [InlineData(32518102, "C", "C]HLA-DRB1*10:01:01:12922]", false, "HLA-DRB1*10:01:01", 12922,     true)]
        public void CreateFromTranslocation_Nominal(int position, string refAllele, string altAllele, bool expectedOnReverseStrand,
            string expectedPartnerChr, int expectedPartnerPosition, bool expectedPartnerOnReverseStrand)
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, position, position, refAllele, altAllele, VariantType.translocation_breakend);
            BreakEndAdjacency[] adjacencies =
                BreakEndAdjacencyFactory.CreateAdjacencies(variant, ChromosomeUtilities.RefNameToChromosome, false, false);

            Assert.NotNull(adjacencies);
            Assert.Single(adjacencies);

            BreakEndAdjacency actual = adjacencies[0];
            Assert.Equal(expectedOnReverseStrand,        actual.Origin.OnReverseStrand);
            Assert.Equal(expectedPartnerChr,             actual.Partner.Chromosome.EnsemblName);
            Assert.Equal(expectedPartnerPosition,        actual.Partner.Position);
            Assert.Equal(expectedPartnerOnReverseStrand, actual.Partner.OnReverseStrand);
        }

        [Fact]
        public void CreateFromTranslocation_InvalidAltAllele_ThrowException()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 100, 100, "A", "A{3:115024109{T", VariantType.translocation_breakend);
                // ReSharper disable once UnusedVariable
                BreakEndAdjacency[] adjacencies = BreakEndAdjacencyFactory.CreateFromTranslocation(variant, ChromosomeUtilities.RefNameToChromosome);
            });
        }

        [Fact]
        public void CreateFromSymbolicAllele_Deletion()
        {
            var                 interval    = new ChromosomeInterval(ChromosomeUtilities.Chr1, 1594584, 1660503);
            BreakEndAdjacency[] adjacencies = BreakEndAdjacencyFactory.CreateFromSymbolicAllele(interval, VariantType.deletion, false, false);

            Assert.NotNull(adjacencies);
            Assert.Equal(2, adjacencies.Length);

            BreakEndAdjacency actual = adjacencies[0];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual.Origin.Chromosome.EnsemblName);
            Assert.Equal(1594583,                              actual.Origin.Position);
            Assert.False(actual.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual.Partner.Chromosome.EnsemblName);
            Assert.Equal(1660504,                              actual.Partner.Position);
            Assert.False(actual.Partner.OnReverseStrand);

            BreakEndAdjacency actual2 = adjacencies[1];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual2.Origin.Chromosome.EnsemblName);
            Assert.Equal(1660504,                              actual2.Origin.Position);
            Assert.True(actual2.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual2.Partner.Chromosome.EnsemblName);
            Assert.Equal(1594583,                              actual2.Partner.Position);
            Assert.True(actual2.Partner.OnReverseStrand);
        }

        [Fact]
        public void CreateFromSymbolicAllele_Duplication()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 37820921, 38404543);
            BreakEndAdjacency[] adjacencies =
                BreakEndAdjacencyFactory.CreateFromSymbolicAllele(interval, VariantType.tandem_duplication, false, false);

            Assert.NotNull(adjacencies);
            Assert.Equal(2, adjacencies.Length);

            BreakEndAdjacency actual = adjacencies[0];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual.Origin.Chromosome.EnsemblName);
            Assert.Equal(38404543,                             actual.Origin.Position);
            Assert.False(actual.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual.Partner.Chromosome.EnsemblName);
            Assert.Equal(37820920,                             actual.Partner.Position);
            Assert.False(actual.Partner.OnReverseStrand);

            BreakEndAdjacency actual2 = adjacencies[1];
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual2.Origin.Chromosome.EnsemblName);
            Assert.Equal(37820920,                             actual2.Origin.Position);
            Assert.True(actual2.Origin.OnReverseStrand);
            Assert.Equal(ChromosomeUtilities.Chr1.EnsemblName, actual2.Partner.Chromosome.EnsemblName);
            Assert.Equal(38404543,                             actual2.Partner.Position);
            Assert.True(actual2.Partner.OnReverseStrand);
        }

        [Fact]
        public void CreateFromSymbolicAllele_Inversion()
        {
            var expectedAdjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 63989115, false), // 63989116 + (+1 offset)
                new BreakPoint(ChromosomeUtilities.Chr1, 64291267, true)); // 64291267 - (0 offset)

            var expectedAdjacency2 = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 64291268, true),   // 64291268 - (0 offset)
                new BreakPoint(ChromosomeUtilities.Chr1, 63989116, false)); // 63989117 + (+1 offset)

            var                 interval    = new ChromosomeInterval(ChromosomeUtilities.Chr1, 63989116, 64291267);
            BreakEndAdjacency[] adjacencies = BreakEndAdjacencyFactory.CreateFromSymbolicAllele(interval, VariantType.inversion, false, false);

            Assert.NotNull(adjacencies);
            Assert.Equal(2,                  adjacencies.Length);
            Assert.Equal(expectedAdjacency,  adjacencies[0]);
            Assert.Equal(expectedAdjacency2, adjacencies[1]);
        }

        [Fact]
        public void CreateFromSymbolicAllele_Inversion_INV3()
        {
            var expectedAdjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 63989115, false), // GOOD
                new BreakPoint(ChromosomeUtilities.Chr1, 64291267, true)); // GOOD

            var expectedAdjacency2 = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 64291267, false),
                new BreakPoint(ChromosomeUtilities.Chr1, 63989115, true));

            var                 interval    = new ChromosomeInterval(ChromosomeUtilities.Chr1, 63989116, 64291267);
            BreakEndAdjacency[] adjacencies = BreakEndAdjacencyFactory.CreateFromSymbolicAllele(interval, VariantType.inversion, true, false);

            Assert.NotNull(adjacencies);
            Assert.Equal(2,                  adjacencies.Length);
            Assert.Equal(expectedAdjacency,  adjacencies[0]);
            Assert.Equal(expectedAdjacency2, adjacencies[1]);
        }

        [Fact]
        public void CreateFromSymbolicAllele_Inversion_INV5()
        {
            var expectedAdjacency = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 63989116, true), 
                new BreakPoint(ChromosomeUtilities.Chr1, 64291268, false)); 

            var expectedAdjacency2 = new BreakEndAdjacency(
                new BreakPoint(ChromosomeUtilities.Chr1, 64291268, true), // GOOD
                new BreakPoint(ChromosomeUtilities.Chr1, 63989116, false)); // GOOD

            var                 interval    = new ChromosomeInterval(ChromosomeUtilities.Chr1, 63989116, 64291267);
            BreakEndAdjacency[] adjacencies = BreakEndAdjacencyFactory.CreateFromSymbolicAllele(interval, VariantType.inversion, false, true);

            Assert.NotNull(adjacencies);
            Assert.Equal(2,                  adjacencies.Length);
            Assert.Equal(expectedAdjacency,  adjacencies[0]);
            Assert.Equal(expectedAdjacency2, adjacencies[1]);
        }

        [Fact]
        public void CreateFromSymbolicAllele_UnhandledVariantType_ReturnNull()
        {
            var interval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 63989116, 64291267);
            BreakEndAdjacency[] adjacencies =
                BreakEndAdjacencyFactory.CreateFromSymbolicAllele(interval, VariantType.complex_structural_alteration, false, false);

            Assert.Null(adjacencies);
        }
    }
}