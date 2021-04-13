using Genome;
using Intervals;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsgNotationTests
    {
        private static readonly ISequence SimpleSequence     = new SimpleSequence("ATCGGTGCTGACGATACCTGACGTAAGTA");
        private readonly        IInterval _referenceInterval = new Interval(0, SimpleSequence.Length);
        private const           string    RefSeqAccession    = "NC_012920.1";

        [Theory]
        [InlineData(5, 5, "G", "T", VariantType.SNV, "NC_012920.1:m.5G>T")]
        [InlineData(5, 5, "G", "G", VariantType.SNV, "NC_012920.1:m.5=")]
        [InlineData(5, 7, "GTG", "", VariantType.deletion, "NC_012920.1:m.5_7del")]
        [InlineData(10, 12, "GAC", "", VariantType.deletion, "NC_012920.1:m.12_14del")]
        [InlineData(16, 15, "", "GATA", VariantType.insertion, "NC_012920.1:m.15_16insGATA")]
        [InlineData(19, 22, "TGAC", "GTCA", VariantType.MNV, "NC_012920.1:m.19_22invTGAC")]
        [InlineData(10, 9, "", "GAC", VariantType.insertion, "NC_012920.1:m.12_14dupCGA")]
        public void GetNotation_MT(int start, int end, string referenceAllele, string altAllele, VariantType type, string expectedHgvs)
        {
            var    simpleVariant = new SimpleVariant(ChromosomeUtilities.ChrM, start, end, referenceAllele, altAllele, type);
            string actualHgvs    = HgvsgNotation.GetNotation(RefSeqAccession, simpleVariant, SimpleSequence, _referenceInterval);
            Assert.Equal(expectedHgvs, actualHgvs);
        }
    }
}