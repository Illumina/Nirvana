using Genome;
using UnitTests.TestUtilities;
using VariantAnnotation.NSA;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.NSA
{
    public sealed class SuppIntervalUtilitiesTests
    {
        [Theory]
        [InlineData(1, 100, 51, 200, 0.33333, 0.33333)]
        [InlineData(1, 300, 51, 200, 0.5, 1)]
        [InlineData(101, 300, 51, 200, 0.5, 0.66667)]
        [InlineData(1, 100, 100, 299, 0.005, 0.005)]
        public void GetOverlapFractions_NotNull_AsExpected(int varStart, int varEnd, int saStart, int saEnd, double expectedReciprocalOverlap, double expecedAnnotationOverlap)
        {
            var saInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, saStart, saEnd);
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, varStart, varEnd, null, null, VariantType.deletion);
            var (reciprocalOverlap, annotationOverlap) = SuppIntervalUtilities.GetOverlapFractions(saInterval, variant);

            Assert.NotNull(reciprocalOverlap);
            Assert.NotNull(annotationOverlap);
            Assert.Equal(expectedReciprocalOverlap, reciprocalOverlap.Value, 5);
            Assert.Equal(expecedAnnotationOverlap, annotationOverlap.Value, 5);
        }

        [Fact]
        public void GetOverlapFractions_ReturnNulls_DifferentChroms()
        {
            var saInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 1, 2);
            var variant = new SimpleVariant(ChromosomeUtilities.Chr2, 1, 2, null, null, VariantType.deletion);
            var (reciprocalOverlap, annotationOverlap) = SuppIntervalUtilities.GetOverlapFractions(saInterval, variant);

            Assert.Null(reciprocalOverlap);
            Assert.Null(annotationOverlap);
        }

        [Fact]
        public void GetOverlapFractions_ReturnNulls_Insertion()
        {
            var saInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 1, 2);
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1, 2, null, null, VariantType.insertion);
            var (reciprocalOverlap, annotationOverlap) = SuppIntervalUtilities.GetOverlapFractions(saInterval, variant);

            Assert.Null(reciprocalOverlap);
            Assert.Null(annotationOverlap);
        }

        [Fact]
        public void GetOverlapFractions_ReturnNulls_SaInsertion()
        {
            var saInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 2, 1);
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1, 2, null, null, VariantType.deletion);
            var (reciprocalOverlap, annotationOverlap) = SuppIntervalUtilities.GetOverlapFractions(saInterval, variant);

            Assert.Null(reciprocalOverlap);
            Assert.Null(annotationOverlap);
        }

        [Fact]
        public void GetOverlapFractions_ReturnNulls_BreakEnd()
        {
            var saInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 2, 1);
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1, 2, null, null, VariantType.translocation_breakend);
            var (reciprocalOverlap, annotationOverlap) = SuppIntervalUtilities.GetOverlapFractions(saInterval, variant);

            Assert.Null(reciprocalOverlap);
            Assert.Null(annotationOverlap);
        }
    }
}